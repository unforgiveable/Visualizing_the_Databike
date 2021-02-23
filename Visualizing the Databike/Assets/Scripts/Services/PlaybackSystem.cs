using SmidtFramework.LoggingSystem;
using SmidtFramework.InputSystem;
using System;
using UnityEngine;
using VisualizingTheDatabike.DataModel;
using VisualizingTheDatabike.Controllers;
using SmidtFramework.ControllerSystem;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// System managing playing / pausing of the timeline. Interacts with the SampleSystem to update the scene during playback.
	/// </summary>
	public class PlaybackSystem
	{
		public float TimelineLength { get; private set; }
		public bool IsCurrentlyPlaying { get; private set; }

		private SampleSystem sampleSystem;
		private IBikePlaybackVisualizer sceneController;

		public PlaybackSystem(InterpolatedTimeline interpolatedTimeline)
		{
			TimelineLength = interpolatedTimeline.Length;
			IsCurrentlyPlaying = false;
			sampleSystem = new SampleSystem(interpolatedTimeline);

			sceneController = ControllerMaster.instance.GetCurrentController<SceneController>("scene");
		}

		#region public_functions
		/// <summary>
		/// Starts timeline playback if it is currently paused, starts it otherwise.
		/// </summary>
		public void SwitchPlayback()
		{
			if (IsCurrentlyPlaying)
				PausePlayback();
			else
				StartPlayback();
		}

		/// <summary>
		/// Pauses timeline playback at the currently set time. Does nothing if already paused.
		/// </summary>
		public void PausePlayback()
		{
			if (!IsCurrentlyPlaying) return;

			if (GlobalValues.DebugPlaybackSystem)
				Log("Paused playback.");

			ForcePausePlayback();
		}

		/// <summary>
		/// Pauses the timeline even if it is already paused, causing all the events to trigger as if it was running before.
		/// </summary>
		public void ForcePausePlayback()
		{
			IsCurrentlyPlaying = false;
			//LSLogger.Log("PlaybackSystem", "Paused playback.");
			UpdateSystem.instance.RemoveFixedUpdateAction("timeline_playback");

			sceneController.UpdatePlaybackState(IsCurrentlyPlaying);
		}

		/// <summary>
		/// Starts timeline playback at the currently set time. Does nothing if already playing.
		/// </summary>
		public void StartPlayback()
		{
			if (IsCurrentlyPlaying) return;

			if (GlobalValues.DebugPlaybackSystem)
				Log("Started playback.");
			
			IsCurrentlyPlaying = true;

			UpdateSystem.instance.AddFixedUpdateAction("timeline_playback", () =>
			{
				if (!IsCurrentlyPlaying) return;
				UpdateSceneWithNextSample();
			});

			sceneController.UpdatePlaybackState(IsCurrentlyPlaying);
		}

		/// <summary>
		/// Retrieves the next sample from the SampleSystem and passes it to the rest of the scene. Does nothing if the end of the timeline is reached.
		/// </summary>
		public void UpdateSceneWithNextSample()
		{
			try
			{
				BikeState newState = sampleSystem.GetNextSample();
				sceneController.UpdateWithNewBikeState(newState);
			}
			catch (ArgumentOutOfRangeException) //end of timeline reached
			{
				LSLogger.Log("PlaybackSystem", "Automatically paused playback as end of timeline is reached.");
				PausePlayback();
				return;
			}
		}

		/// <summary>
		/// Sets the current timeline playback time in seconds from timeline start.
		/// </summary>
		/// <param name="newTime"></param>
		public void SetCurrentTime(float newTime)
		{
			if (newTime < 0 || newTime > TimelineLength)
			{
				LSLogger.LogError("PlaybackSystem SetCurrentTime newTime out of range.");
				throw new ArgumentException("newTime out of range.");
			}

			if (GlobalValues.DebugPlaybackSystem)
				Log("Set time to " + newTime);

			sampleSystem.SetCurrentTime(newTime);

			//update the scene with the new time
			UpdateSceneWithNextSample();

			ForcePausePlayback();
		}

		/// <summary>
		/// Updates the scene at the current time but does not advance the timeline like UpdateSceneWithNextSample() would.
		/// </summary>
		public void UpdateAtCurrentTime()
		{
			float lastTime = sampleSystem.CurrentTime - sampleSystem.CurrentStepSize;
			if (lastTime < 0f) lastTime = 0f;
			SetCurrentTime(lastTime);
		}

		/// <summary>
		/// Sets the current playback speed.
		/// </summary>
		/// <param name="newSpeedMult"></param>
		public void SetCurrentSpeed(float newSpeedMult)
		{
			sampleSystem.SetReplaySpeed(newSpeedMult);
		}

		/// <summary>
		/// Invalidates all currently pre-computed samples.
		/// </summary>
		public void InvalidateSamples()
		{
			if (GlobalValues.DebugPlaybackSystem)
				Log("Invalidated samples.");

			sampleSystem.InvalidateSamples();
		}

		/// <summary>
		/// Delegates the call to the current SampleSystem.
		/// </summary>
		/// <param name="samplesPerSecond">Number of samples to be computed for each second of the timeline.</param>
		/// <returns>An array of positions that are sorted ascending by time.</returns>
		public Vector3[] GetTrailSamples(int samplesPerSecond)
		{
			return sampleSystem.GetTrailSamples(samplesPerSecond);
		}
		#endregion


		private void Log(string message)
		{
			LSLogger.Log("PlaybackSystem", message);
		}
	}
}