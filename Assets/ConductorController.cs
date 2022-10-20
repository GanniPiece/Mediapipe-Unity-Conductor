using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

namespace Mediapipe.Unity
{
public class ConductorController : MonoBehaviour
{
    float prevTime;
    Vector2 cur, prev, pprev;
    float prevDistance, curDistance;
    float prevSpeed;
    bool isBeat;
    bool isAsc;
    bool isDirty;
    AudioSource _click;

    void Start ()
    {
        _click = GetComponent<AudioSource>();
        isBeat = false;
        isAsc = false;
        isDirty = false;
    }

    void Update()
    {
        Calc();
        if (OnBeat()) {
            Click();
        }
    }

    private void Click ()
    {
        if (_click.isPlaying)
            _click.Stop();
        _click.Play();
    }

    private void Calc ()
    {
        var now = Time.time;
        if (now - prevTime < 0.3) return;
        if (Vector2.Distance(prev, cur) < 0.05) return;
        if (!isDirty) return;

        var curSpeed = curDistance / (now - prevTime);

        // (!asc -> asc) beat (!asc)
        if ( (isAsc && curSpeed < prevSpeed) && 
            (((pprev.x - prev.x) * (prev.x - cur.x) < 0) ||
             ((pprev.y - prev.y) * (prev.y - cur.y) < 0)) )
        {
            Debug.Log("accelerating");
            isBeat = true;
            isAsc = false;
        }
        else if (!isAsc && curSpeed > prevSpeed)
        {
            Debug.Log("de-accelerating");
            isAsc = true;
        }


        pprev = prev;
        prev = cur;
        prevSpeed = curSpeed;
        prevTime = now;
        isDirty = false;
    }

    private bool OnBeat ()
    {
        if (isBeat)
        {
            isBeat = false;
            return true;
        }

        return false;
    }

    public void Process (List<NormalizedLandmarkList> landmarkLists)
    {
        if (landmarkLists == null || landmarkLists.Count <= 0) return;

        var firstHand = landmarkLists[0];
        var landmark = firstHand.Landmark[8];
        cur = new Vector2(landmark.X, landmark.Y);

        // if (Vector2.Distance(cur, last1) < 0.05) return;
        
        curDistance = Vector3.Distance(cur, prev);

        isDirty = true;
    }
}
}