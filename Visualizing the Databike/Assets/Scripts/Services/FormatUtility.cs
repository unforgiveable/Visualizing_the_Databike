
namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Class for utility formatting methods.
	/// </summary>
	public static class FormatUtility
	{
		/// <summary>
		/// Formats a time in seconds as HH:MM:SS.SSS.
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static string GetTimeStringFromSeconds(float seconds)
		{
			float t = seconds;
			int hours = (int)(t / 3600);
			t -= hours * 3600;
			int minutes = (int)(t / 60);
			t -= minutes * 60;
			return string.Format("{0:00}:{1:00}:{2:00.000}", hours, minutes, t);
		}
	}
}
