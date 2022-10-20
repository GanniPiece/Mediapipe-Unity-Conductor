// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

// ATTENTION!: This code is for a tutorial.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mediapipe.Unity.Tutorial
{
  public class FaceMesh : MonoBehaviour
  {
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;

    private WebCamTexture _webCamTexture;
    private Texture2D _inputTexture;
    private Color32[] _inputPixelData;
    private CalculatorGraph _graph;

    private ResourceManager _resourceManager;

    private Texture2D _outputTexture;
    private Color32[] _outputPixelData;

    private IEnumerator Start()
    {
      if (WebCamTexture.devices.Length == 0)
      {
        throw new System.Exception("Web Camera devices are not found");
      }
      var webCamDevice = WebCamTexture.devices[1];
      _webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
      _webCamTexture.Play();

      yield return new WaitUntil(() => _webCamTexture.width > 16);

      _screen.rectTransform.sizeDelta = new Vector2(_width, _height);

      _inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
      _inputPixelData = new Color32[_width * _height];
      _outputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
      _outputPixelData = new Color32[_width * _height];

      _screen.texture = _outputTexture;

      _resourceManager = new LocalResourceManager();
      yield return _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
      yield return _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");

      var stopwatch = new Stopwatch();

      // MEDIA PIPE GRAPH
      _graph = new CalculatorGraph(_configAsset.text);
      var outputVideoStream = new OutputStream<ImageFramePacket, ImageFrame>(_graph, "output_video");
      var multiFaceLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
      outputVideoStream.StartPolling().AssertOk();
      multiFaceLandmarksStream.StartPolling().AssertOk();
      _graph.StartRun().AssertOk();
      stopwatch.Start();

      var screenRect = _screen.GetComponent<RectTransform>().rect;

      while (true)
      {
        _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
        var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
        var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
        _graph.AddPacketToInputStream("input_video", new ImageFramePacket(imageFrame, new Timestamp(currentTimestamp))).AssertOk();
        
        yield return new WaitForEndOfFrame(); 

        if (outputVideoStream.TryGetNext(out var outputVideo))
        {
          if (outputVideo.TryReadPixelData(_outputPixelData))
          {
            _outputTexture.SetPixels32(_outputPixelData);
            _outputTexture.Apply();
          }
        }

        if (multiFaceLandmarksStream.TryGetNext(out var multiFaceLandmarks))
        {
          if (multiFaceLandmarks != null && multiFaceLandmarks.Count > 0)
          {
            foreach (var landmarks in multiFaceLandmarks)
            {
              // top of the head
              var topOfHead = landmarks.Landmark[10];
              Debug.Log($"Unity Local Coordinates: {screenRect.GetPoint(topOfHead)}, ImageCoordinates: {topOfHead}");
            }
          }
        }
      }
    }

    private void OnDestroy()
    {
      if (_webCamTexture != null)
      {
        _webCamTexture.Stop();
      }

      if (_graph != null)
      {
        try
        {
          _graph.CloseInputStream("input_video").AssertOk();
          _graph.WaitUntilDone().AssertOk();
        }
        finally
        {
          _graph.Dispose();
        }
      }
    }
  }
}
