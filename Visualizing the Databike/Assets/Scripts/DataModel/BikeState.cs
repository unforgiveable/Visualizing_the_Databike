using UnityEngine;

namespace VisualizingTheDatabike.DataModel
{
	/// <summary>
	/// Class describing the state of a bike; Modified by the sample system and read from by all visual controllers.
	/// </summary>
	public class BikeState
	{
		public float Time { get; set; } //relative time from start in seconds

		//timeline configurable values
		public float Pedaloffset { get; set; } //in deg from forward [0 - 360]

		//runtime values describing current state in in-engine units
		public Vector3 Position { get; set; } //in-scene position
		public Vector3 Rotation { get; set; } //in-scene rotation
		public float WheelRPM { get; set; } //in revolutions/minute
		public float SteeringRotation { get; set; } //in degrees [-180 - +180]
		public float PedalRotation { get; set; } //in degrees clockwise
		public int GearFront { get; set; } //[1-frontgears]
		public int GearRear { get; set; } //[1-reargears]
		public float BrakeRight { get; set; } //in percent [0.00 - 1.00] of max with 0 = at rest
		public float BrakeLeft { get; set; } //in percent [0.00 - 1.00] of max with 0 = at rest
		public float SuspensionFront { get; set; } //in percent [0.00 - 1.00] of max with 0 = at rest
		public float SuspensionRear { get; set; } //in percent [0.00 - 1.00] of max with 0 = at rest
		public float SeatPosition { get; set; } //in percent [0.00 - 1.00] of max with 0 = at rest
		public float SpeedMPS { get; set; } //computed speed in meters/second

		public override string ToString()
		{
			return "BikeState: Time " + Time + "; Pedaloffset " + Pedaloffset + "; Position " + Position.ToString() + "; Rotation " + Rotation.ToString() + "; WheelRPM " + WheelRPM + "; SteeringRotation " + SteeringRotation + "; PedalRotation " + PedalRotation + "; GearFront " + GearFront + "; GearRear " + GearRear + "; BrakeRight " + BrakeRight + "; BrakeLeft " + BrakeLeft + "; SuspensionFront " + SuspensionFront + "; SuspensionRear " + SuspensionRear + "; SeatPosition " + SeatPosition + "; SpeedMPS " + SpeedMPS;
		}
	}
}

