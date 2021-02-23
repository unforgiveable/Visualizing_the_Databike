using SmidtFramework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TestMySpline;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Class for converting a RawTimeline into an InterpolatedTimeline.
	/// </summary>
	public class TimelineConverter
	{
		private const long fileTimeToSec = 10000000;

		public static InterpolatedTimeline GetInterpolatedFromRawTimeline(RawTimeline raw)
		{
			InterpolatedTimeline interpolated = new InterpolatedTimeline()
			{
				Name = raw.Name,
				Bikename = raw.Bikename,
				Pedaloffset = raw.Pedaloffset,
				StartTime = raw.Time.Count > 0 ? raw.Time[0] : 0,
				EndTime = raw.Time.Count > 0 ? raw.Time[raw.Time.Count - 1] : 0
			};

			//convert timestamps to offset in seconds from startTime (for increased accuracy using floats)
			interpolated.Length = (interpolated.EndTime - interpolated.StartTime) / fileTimeToSec;
			float[] relativeTime = raw.Time.Select(x => Convert.ToSingle(Convert.ToDouble(x - interpolated.StartTime) / fileTimeToSec)).ToArray();

			//position
			interpolated.PosXSpline = new CubicSpline(relativeTime, raw.PosX.ToArray());
			interpolated.PosYSpline = new CubicSpline(relativeTime, raw.PosY.ToArray());
			interpolated.PosZSpline = new CubicSpline(relativeTime, raw.PosZ.ToArray());

			//rotation
			interpolated.RotXSpline = new CubicSpline(relativeTime, raw.RotX.ToArray());
			interpolated.RotYSpline = new CubicSpline(relativeTime, raw.RotY.ToArray());
			interpolated.RotZSpline = new CubicSpline(relativeTime, raw.RotZ.ToArray());

			//basic values
			interpolated.WheelrpmSpline = new CubicSpline(relativeTime, raw.Wheelrpm.ToArray());
			interpolated.SteeringrotSpline = new CubicSpline(relativeTime, raw.Steeringrot.ToArray());
			interpolated.PedalrotSpline = new CubicSpline(relativeTime, raw.Pedalrot.ToArray());
			interpolated.BrakerightSpline = new CubicSpline(relativeTime, raw.Brakeright.ToArray());
			interpolated.BrakeleftSpline = new CubicSpline(relativeTime, raw.Brakeleft.ToArray());
			interpolated.SuspfrontSpline = new CubicSpline(relativeTime, raw.Suspfront.ToArray());
			interpolated.SusprearSpline = new CubicSpline(relativeTime, raw.Susprear.ToArray());
			interpolated.SeatposSpline = new CubicSpline(relativeTime, raw.Seatpos.ToArray());

			//gears
			interpolated.GearfrontList = new List<Pair<float, int>>();
			interpolated.GearrearList = new List<Pair<float, int>>();

			interpolated.GearfrontList.Add(new Pair<float, int>(relativeTime[0], raw.Gearfront[0]));
			interpolated.GearrearList.Add(new Pair<float, int>(relativeTime[0], raw.Gearrear[0]));
			//only add to list if different from last one
			for (int i = 1; i < relativeTime.Length; i++)
			{
				if (raw.Gearfront[i] != raw.Gearfront[i - 1])
					interpolated.GearfrontList.Add(new Pair<float, int>(relativeTime[i], raw.Gearfront[i]));
				if (raw.Gearrear[i] != raw.Gearrear[i - 1])
					interpolated.GearrearList.Add(new Pair<float, int>(relativeTime[i], raw.Gearrear[i]));
			}

			return interpolated;
		}
	}
}

