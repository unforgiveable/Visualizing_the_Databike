using SmidtFramework.ControllerSystem;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for steering rotation indicator UI. ID "steering_indicator".
	/// </summary>
	public class SteeringIndicatorController : BaseUIController, IBikeVisualizer
	{
		private RectTransform indicatorTransform;
		private Text angleText;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/SteeringIndicator";
		}

		public override void Init()
		{
			//get references
			indicatorTransform = WorldObject.transform.GetChild(0).GetComponent<RectTransform>();
			angleText = WorldObject.transform.GetChild(1).GetChild(0).GetComponent<Text>();
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
		/// Sets the indicator needle to point at the new steering rotation angle and updates the angle text. Gets called when the scene gets updated with a new BikeState.
		/// </summary>
		/// <param name="newState"></param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			float needleAngle = -(newState.SteeringRotation % 360);
			//limit to fit onto dial
			if (needleAngle < -140) needleAngle = -140;
			else if (needleAngle > 140) needleAngle = 140;

			indicatorTransform.rotation = Quaternion.Euler(0, 0, needleAngle);

			angleText.text = string.Format("{0:000.0}°", newState.SteeringRotation);
		}
	}
}
