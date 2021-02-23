using SmidtFramework.LoggingSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// System for sampling states from an InterpolatedTimeline. Internally pre-computes and caches samples a fixed stepsize apart.
	/// </summary>
	public class SampleSystem
	{
		#region fields
		private InterpolatedTimeline interpolatedTimeline;

		private const int cacheSize = 50;
		
		//datastructures for computing and caching samples by reusing (almost) all memory
		private List<BikeState> newSamples;
		private List<BikeState> oldSamples;
		private float[] timeStamps;
		private float[][] valueSamples;
		private Vector3? prevPosition; //previous bike position used for computing current speed
		
		public float CurrentTime { get; private set; } = 0;
		public float CurrentStepSize { get; private set; } = Time.fixedDeltaTime;
		private float currentSpeedMult = 1f;
		#endregion

		/// <summary>
		/// Creates a new SampleSystem to use the provided InterpolatedTimeline.
		/// </summary>
		/// <param name="interpolatedTimeline"></param>
		public SampleSystem(InterpolatedTimeline interpolatedTimeline)
		{
			this.interpolatedTimeline = interpolatedTimeline;
			newSamples = new List<BikeState>();
			oldSamples = new List<BikeState>();

			//create bike samples which will be reused over time
			for (int i = 0; i < cacheSize; i++)
			{
				oldSamples.Add(new BikeState());
			}

			//init datastructes for sample computation
			timeStamps = new float[cacheSize];
			valueSamples = new float[14][];
			for (int i = 14; i < 14; i++)
			{
				valueSamples[i] = new float[cacheSize];
			}
		}

		#region public_functions
		/// <summary>
		/// Returns the next BikeState with the current replay settings.
		/// </summary>
		public BikeState GetNextSample()
		{
			bool DEBUG = GlobalValues.DebugSampleSystem;
			if (DEBUG) LSLogger.Log("SampleSystem", "GetNextSample has " + newSamples.Count + " samples cached.");
			
			if (CurrentTime >= interpolatedTimeline.Length)
			{
				if (DEBUG) LSLogger.Log("SampleSystem", "End of timeline at " + CurrentTime + " sec.");
				throw new ArgumentOutOfRangeException("timeline end reached.");
			}

			if (newSamples.Count == 0)
			{
				ComputeSamples();
				if (newSamples.Count == 0)
				{
					LSLogger.LogError("Error computing new samples.");
					return null;
				}
				if (DEBUG) LSLogger.Log("SampleSystem", "New samples compute successful.");
			}

			BikeState sample = newSamples[0];
			newSamples.RemoveAt(0);
			oldSamples.Add(sample);

			CurrentTime += CurrentStepSize;

			if (DEBUG) LSLogger.Log("SampleSystem", "New sample retrieved at time " + sample.Time + ".");
			if (GlobalValues.DebugSampleSystemExtended) LSLogger.Log("SampleSystem", "New sample: " + sample.ToString());
			return sample;
		}

		/// <summary>
		/// Changes the current position in the timeline.
		/// </summary>
		/// <param name="newTime">New time in the timeline in seconds from the start.</param>
		public void SetCurrentTime(float newTime)
		{
			CurrentTime = newTime;
			InvalidateSamples();
		}

		/// <summary>
		/// Changes the playback speed.
		/// </summary>
		/// <param name="newSpeed">Playback speed multiplier, 1 = real-time</param>
		public void SetReplaySpeed(float newSpeed)
		{
			CurrentStepSize = Time.fixedDeltaTime * newSpeed;
			currentSpeedMult = newSpeed;
			InvalidateSamples();
		}

		/// <summary>
		/// Computes a list of positions to be used by the TrailController. Works intependently from the runtime sample computation system.
		/// </summary>
		/// <param name="samplesPerSecond">Number of samples to be computed for each second of the timeline.</param>
		/// <returns>An array of positions that are sorted ascending by time.</returns>
		public Vector3[] GetTrailSamples(int samplesPerSecond)
		{
			float timeStep = 1f / samplesPerSecond;
			int steps = (int) (interpolatedTimeline.Length / timeStep);
			float[] times = new float[steps];
			for (int i = 0; i < steps; i++)
			{
				times[i] = i * timeStep;
			}

			float[] xs = interpolatedTimeline.PosXSpline.Eval(times);
			float[] ys = interpolatedTimeline.PosYSpline.Eval(times);
			float[] zs = interpolatedTimeline.PosZSpline.Eval(times);

			Vector3[] positions = new Vector3[steps];
			for (int i = 0;  i< steps; i++)
			{
				positions[i] = new Vector3(xs[i], ys[i], zs[i]);
			}

			return positions;
		}
		/// <summary>
		/// Invalidates all currently pre-computed samples
		/// </summary>
		public void InvalidateSamples()
		{
			for (int i = newSamples.Count-1; i >= 0; i--)
			{
				oldSamples.Add(newSamples[i]);
				newSamples.RemoveAt(i);
			}
			prevPosition = null;
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Computes the next cacheSize number of BikeStates for the current timeline playback settings. Reuses BikeState instances as well as temporary value arrays for improved memory efficiency. Throws an ArgumentException if oldSamples does not contain cacheSize number of BikeStates to reuse.
		/// </summary>
		private void ComputeSamples()
		{
			bool DEBUG = GlobalValues.DebugSampleSystem;
			if (DEBUG) LSLogger.Log("SampleSystem", "ComputeSamples has " + oldSamples.Count + " old samples available.");
			if (oldSamples.Count < cacheSize)
			{
				throw new ArgumentException("Compute samples does not have enough BikeStates to use in oldSamples.");
			}

			//compute time stamps for each sample
			float time = CurrentTime;
			for (int i = 0; i < cacheSize; i++)
			{
				if (time >= interpolatedTimeline.Length) timeStamps[i] = 0;
				timeStamps[i] = time;
				time += CurrentStepSize;
			}

			//get interpolated values for each property
			valueSamples[0] = interpolatedTimeline.PosXSpline.Eval(timeStamps);
			valueSamples[1] = interpolatedTimeline.PosYSpline.Eval(timeStamps);
			valueSamples[2] = interpolatedTimeline.PosZSpline.Eval(timeStamps);

			valueSamples[3] = interpolatedTimeline.RotXSpline.Eval(timeStamps);
			valueSamples[4] = interpolatedTimeline.RotYSpline.Eval(timeStamps);
			valueSamples[5] = interpolatedTimeline.RotZSpline.Eval(timeStamps);

			valueSamples[6] = interpolatedTimeline.WheelrpmSpline.Eval(timeStamps);
			valueSamples[7] = interpolatedTimeline.SteeringrotSpline.Eval(timeStamps);
			valueSamples[8] = interpolatedTimeline.PedalrotSpline.Eval(timeStamps);
			valueSamples[9] = interpolatedTimeline.BrakerightSpline.Eval(timeStamps);
			valueSamples[10] = interpolatedTimeline.BrakeleftSpline.Eval(timeStamps);
			valueSamples[11] = interpolatedTimeline.SuspfrontSpline.Eval(timeStamps);
			valueSamples[12] = interpolatedTimeline.SusprearSpline.Eval(timeStamps);
			valueSamples[13] = interpolatedTimeline.SeatposSpline.Eval(timeStamps);

			if (DEBUG) LSLogger.Log("SampleSystem", "ComputeSamples got interpolated values.");

			//find index in list of gear changes for current time (=start time of new samples)
			int gearFrontIndex = interpolatedTimeline.GearfrontList.Count - 1;
			bool gearFrontEnd = true; //true if last index in list is reached
			int gearRearIndex = interpolatedTimeline.GearrearList.Count - 1;
			bool gearRearEnd = true; //true if last index in list is reached
			for (int i = 1; i < interpolatedTimeline.GearfrontList.Count; i++)
			{
				if (interpolatedTimeline.GearfrontList[i].First > CurrentTime)
				{
					gearFrontIndex = i - 1;
					gearFrontEnd = false;
					break;
				}
			}
			for (int i = 1; i < interpolatedTimeline.GearrearList.Count; i++)
			{
				if (interpolatedTimeline.GearrearList[i].First > CurrentTime)
				{
					gearRearIndex = i - 1;
					gearRearEnd = false;
					break;
				}
			}

			if (DEBUG) LSLogger.Log("SampleSystem", "ComputeSamples found gear list indices. ("+gearFrontEnd+", "+gearRearEnd+")");

			BikeState lateUpdateSpeedState = null; //used for reusing the speed of the second sample for the first one
			//compute new samples
			for (int i = 0; i < cacheSize; i++)
			{
				BikeState state = oldSamples[0];
				oldSamples.RemoveAt(0);

				state.Time = timeStamps[i];

				//retrieve current offset from global values
				state.Pedaloffset = GlobalValues.PedalOffset;

				state.Position = new Vector3(valueSamples[0][i], valueSamples[1][i], valueSamples[2][i]);
				state.Rotation = new Vector3(valueSamples[3][i], valueSamples[4][i], valueSamples[5][i]);

				state.WheelRPM = valueSamples[6][i] * currentSpeedMult; //adjust RPM to match replay speed
				state.SteeringRotation = valueSamples[7][i];
				state.PedalRotation = valueSamples[8][i];
				state.BrakeRight = ClampFloat(valueSamples[9][i], 0f, 1f);
				state.BrakeLeft = ClampFloat(valueSamples[10][i], 0f, 1f);
				state.SuspensionFront = ClampFloat(valueSamples[11][i], 0f, 1f);
				state.SuspensionRear = ClampFloat(valueSamples[12][i], 0f, 1f);
				state.SeatPosition = ClampFloat(valueSamples[13][i], 0f, 1f);

				//Check for gear change - assumes only one change possible within a timestep
				if (!gearFrontEnd)
				{
					if (timeStamps[i] >= interpolatedTimeline.GearfrontList[gearFrontIndex + 1].First)
					{
						gearFrontIndex++;
						if (gearFrontIndex == interpolatedTimeline.GearfrontList.Count - 1)
							gearFrontEnd = true;
					}
				}
				if (!gearRearEnd)
				{
					if (timeStamps[i] >= interpolatedTimeline.GearrearList[gearRearIndex + 1].First)
					{
						gearRearIndex++;
						if (gearRearIndex == interpolatedTimeline.GearrearList.Count - 1)
							gearRearEnd = true;
					}
				}

				state.GearFront = interpolatedTimeline.GearfrontList[gearFrontIndex].Second;
				state.GearRear = interpolatedTimeline.GearrearList[gearRearIndex].Second;

				//Compute current speed from location change between previous and current sample
				if (prevPosition == null)
				{
					lateUpdateSpeedState = state; //first sample - reuse speed from next one
				}
				else
				{
					float speed = (state.Position - prevPosition.Value).magnitude * (1f/CurrentStepSize);
					state.SpeedMPS = speed;
					if (lateUpdateSpeedState != null)
					{
						lateUpdateSpeedState.SpeedMPS = speed;
						lateUpdateSpeedState = null;
					}
				}
				prevPosition = state.Position;

				newSamples.Add(state);
			}

			if (DEBUG) LSLogger.Log("SampleSystem", "ComputeSamples computed new samples.");
		}

		/// <summary>
		/// Ensures that value is between min and max.
		/// </summary>
		/// <returns>The value if its within the bounds, the nearest bound otherwise.</returns>
		private float ClampFloat(float value, float min, float max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}
		#endregion

	}
}
