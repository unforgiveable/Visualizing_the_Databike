using System.Collections.Generic;
using TestMySpline;
using SmidtFramework.Utility;

namespace VisualizingTheDatabike.DataModel
{
	/// <summary>
	/// Class to hold the raw information parsed form a timeline file. Used as input for the spline interpolation.
	/// </summary>
	public class RawTimeline
	{
		public string Name { get; set; }
		public string Bikename { get; set; }
		public float Pedaloffset { get; set; }

		public List<long> Time { get; set; } //time as windows system filetime

		//already in scene coordinates / angles
		public List<float> PosX { get; set; } //east
		public List<float> PosY { get; set; } //up
		public List<float> PosZ { get; set; } //north

		public List<float> RotX { get; set; } //east (absolute)
		public List<float> RotY { get; set; } //up (absolute)
		public List<float> RotZ { get; set; } //north (absolute)

		public List<float> Wheelrpm { get; set; }
		public List<float> Steeringrot { get; set; }
		public List<float> Pedalrot { get; set; } //special case - needs to include rotdir in interpolation
		public List<int> Gearfront { get; set; } //wont be interpolated
		public List<int> Gearrear { get; set; } //wont be interpolated
		public List<float> Brakeright { get; set; }
		public List<float> Brakeleft { get; set; }

		//normalized to [0-1] using bikedef values
		public List<float> Suspfront { get; set; }
		public List<float> Susprear { get; set; }
		public List<float> Seatpos { get; set; }

		public RawTimeline()
		{
			Time = new List<long>();
			PosX = new List<float>();
			PosY = new List<float>();
			PosZ = new List<float>();
			RotX = new List<float>();
			RotY = new List<float>();
			RotZ = new List<float>();
			Wheelrpm = new List<float>();
			Steeringrot = new List<float>();
			Pedalrot = new List<float>();
			Gearfront = new List<int>();
			Gearrear = new List<int>();
			Brakeright = new List<float>();
			Brakeleft = new List<float>();
			Suspfront = new List<float>();
			Susprear = new List<float>();
			Seatpos = new List<float>();
		}

		public override string ToString()
		{
			return "RawTimeline " + Name + ":\nTime [" + string.Join(";", Time) + "], \nPosX [" + string.Join(";", PosX) + "], \nPosY [" + string.Join(";", PosY) + "], \nPosZ [" + string.Join(";", PosZ) + "], \nRotx [" + string.Join(";", RotX) + "], \nRoty [" + string.Join(";", RotY) + "], \nRotz [" + string.Join(";", RotZ) + "], \nWheelrpm [" + string.Join(";", Wheelrpm) + "], \nSteeringrot [" + string.Join(";", Steeringrot) + "], \nPedalrot [" + string.Join(";", Pedalrot) + "], \nGearfront [" + string.Join(";", Gearfront) + "], \nGearrear [" + string.Join(";", Gearrear) + "], \nBrakeright [" + string.Join(";", Brakeright) + "], \nBrakeleft [" + string.Join(";", Brakeleft) + "], \nSuspfront [" + string.Join(";", Suspfront) + "], \nSusprear [" + string.Join(";", Susprear) + "], \nSeatpos [" + string.Join(";", Seatpos) + "]";
		}
	}


	/// <summary>
	/// Class for holding the interpolated information, ready to be sampled from at any time between StartTime and EndTime.
	/// </summary>
	public class InterpolatedTimeline
	{
		public string Name { get; set; }
		public string Bikename { get; set; }
		public float Pedaloffset { get; set; }

		public long StartTime { get; set; }
		public long EndTime { get; set; }
		public float Length { get; set; } //total length in seconds

		//interpolated values for relative times, ranging from 0 to (EndTime - StartTime)
		public CubicSpline PosXSpline { get; set; }
		public CubicSpline PosYSpline { get; set; }
		public CubicSpline PosZSpline { get; set; }
		public CubicSpline RotXSpline { get; set; }
		public CubicSpline RotYSpline { get; set; }
		public CubicSpline RotZSpline { get; set; }
		public CubicSpline WheelrpmSpline { get; set; }
		public CubicSpline SteeringrotSpline { get; set; }
		public CubicSpline PedalrotSpline { get; set; }
		public CubicSpline BrakerightSpline { get; set; }
		public CubicSpline BrakeleftSpline { get; set; }
		public CubicSpline SuspfrontSpline { get; set; }
		public CubicSpline SusprearSpline { get; set; }
		public CubicSpline SeatposSpline { get; set; }

		//lookup for non-interpolated values, sorted by relative time in seconds
		public List<Pair<float, int>> GearfrontList { get; set;  }
		public List<Pair<float, int>> GearrearList { get; set;  }
	}

}