using SmidtFramework.ControllerSystem;
using UnityEngine.UI;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the gear indicator UI. ID "gear_indicator".
	/// </summary>
	public class GearsIndicatorController : BaseUIController, IBikeVisualizer
	{
		private Text rearGearText;
		private Text frontGearText;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/GearsIndicator";
		}

		public override void Init()
		{
			//get references to text objects
			rearGearText = WorldObject.transform.GetChild(0).GetComponent<Text>();
			frontGearText = WorldObject.transform.GetChild(1).GetComponent<Text>();
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
		}
		#endregion

		/// <summary>
		/// Updates the gear text with the current gear.
		/// </summary>
		/// <param name="newState"></param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			rearGearText.text = newState.GearRear.ToString();
			frontGearText.text = newState.GearFront.ToString();
		}
	}
}
