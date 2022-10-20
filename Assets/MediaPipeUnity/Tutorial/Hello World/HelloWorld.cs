// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

// ATTENTION!: This code is for a tutorial and it's broken as is.

using UnityEngine;

namespace Mediapipe.Unity.Tutorial
{
	public class HelloWorld : MonoBehaviour 
	{
		private void Start ()
		{
			var configText = @"
			input_stream: ""in""
			output_stream: ""out""
			node {
				calculator: ""PassThroughCalculator""
				input_stream: ""in""
				output_stream: ""out1""
			}
			node {
				calculator: ""PassThroughCalculator""
				input_stream: ""out1""
				output_stream: ""out""
			}
			";
			// var configText = @"Invalid Input";

			// input
			Debug.Log("Start");
			var graph = new CalculatorGraph(configText);
			// var statusOfPoller = graph.AddOutputStreamPoller<string>("out");			
			// if (!statusOfPoller.Ok())
			// {
			// 	graph.Dispose();
			// 	Debug.Log("Graph is disposed.");
			// 	return;
			// }
			// var poller = statusOfPoller.Value();
			var poller = graph.AddOutputStreamPoller<string>("out").Value();
			graph.StartRun().AssertOk();

			for (var i = 0; i < 10; i++)
			{
				var input = new StringPacket("Hello World", new Timestamp(i));
				graph.AddPacketToInputStream("in", input).AssertOk();
			}

			graph.CloseInputStream("in").AssertOk();

			// output
			// var graph = new CalculatorGraph(configText);
			// graph.StartRun().AssertOk();

			graph.CloseInputStream("in").AssertOk();

			var output = new StringPacket();
			while (poller.Next(output))
			{
				Debug.Log(output.Get());
			}

			graph.WaitUntilDone().AssertOk();
			graph.Dispose();
			Debug.Log("Done");
		}
	}
}
