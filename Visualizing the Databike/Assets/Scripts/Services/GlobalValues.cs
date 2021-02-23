
namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Class for storing globally accessible values.
	/// </summary>
	public static class GlobalValues
	{
		public static float PedalOffset { get; set; } = 0f;

		public static bool DebugSampleSystem = false;
		public static bool DebugSampleSystemExtended = false;
		public static bool DebugPlaybackController = false;
		public static bool DebugTimelineReader = false;
		public static bool DebugTimelineReaderExtended = false;
		public static bool DebugPlaybackSystem = false;

		public static bool MirrorConsoleToLogFile = false;
	}
}
