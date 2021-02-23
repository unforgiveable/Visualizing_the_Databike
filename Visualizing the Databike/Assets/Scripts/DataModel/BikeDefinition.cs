
namespace VisualizingTheDatabike.DataModel
{
	/// <summary>
	/// Data structure for a bike definition.
	/// </summary>
	public class BikeDefinition
	{
		public string Name { get; set; }
		public string Prefabpath { get; set; }
		public int Frontgears { get; set; } //number of front gears
		public int Reargears { get; set; } //number of rear gears
		public float Maxfrontsus { get; set; } //in mm
		public float Maxrearsus { get; set; } //in mm
		public float Maxseatpos { get; set; } //in mm
		public int FrontBrake { get; set; } //0 = left, 1 = right
	}
}

