using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Interface for a class to read and parse a timeline from a file.
	/// </summary>
	public interface ITimelineReader
	{
		RawTimeline ReadTimelineFromFile(string filepath, out BikeDefinition bikeDef);
	}
}