using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Interface for controllers that need to update when a new BikeState becomes available.
	/// </summary>
	public interface IBikeVisualizer
	{
		void UpdateWithNewBikeState(BikeState newState);
	}

	/// <summary>
	/// Interface for controllers that need to update when a new BikeState becomes available or if the playback state changes.
	/// </summary>
	public interface IBikePlaybackVisualizer : IBikeVisualizer
	{
		void UpdatePlaybackState(bool newState);
	}
}
