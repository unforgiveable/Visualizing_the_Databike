using SmidtFramework.ControllerSystem;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for managing all status bar UI elements. ID "status_bar".
	/// </summary>
	public class BarController : BaseUIController, IBikeVisualizer
	{
		#region fields
		private const int numBars = 6;
		private const float maxSpeedMPS = 20f;

		private BikeDefinition bikeDefinition;

		private Slider[] sliders;
		private Text[] texts;
		private Image[] infills;

		private Color32 color_base = new Color32(255, 255, 255, 255);
		private Color32 color_75 = new Color32(255, 255, 55, 255);
		private Color32 color_90 = new Color32(255, 40, 40, 255);
		#endregion

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/StatusBarsUI";
		}

		public override void Init()
		{
			sliders = new Slider[numBars];
			texts = new Text[numBars];
			infills = new Image[numBars];

			//get references to all sliders and texts
			for (int i = 0; i < numBars; i++)
			{
				Transform t = WorldObject.transform.GetChild(i);
				sliders[i] = t.GetComponent<Slider>();
				texts[i] = t.GetChild(3).GetComponent<Text>();
				infills[i] = t.GetChild(1).GetChild(0).GetComponent<Image>();
			}

			bikeDefinition = ControllerMaster.instance.GetCurrentController<SceneController>("scene").BikeDefinition;

			//no need to initialize / reset as scene configuration switch triggers update with initial bike state
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

		#region public_functions
		/// <summary>
		/// Updates the status bars with the current bike state. Uses the values from the BikeDefinition to compute absolute units.
		/// </summary>
		/// <param name="newState"></param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			//speed
			sliders[0].value = ClampFloat(newState.SpeedMPS / maxSpeedMPS, 0f, 1f);
			texts[0].text = string.Format("Speed: {0:00.0}m/s | {1:00.0}km/h", newState.SpeedMPS, newState.SpeedMPS * 3.6f);

			//brakes
			float front, rear;
			if (bikeDefinition.FrontBrake == 0)
			{
				front = newState.BrakeLeft;
				rear = newState.BrakeRight;
			}
			else
			{
				front = newState.BrakeRight;
				rear = newState.BrakeLeft;
			}
			sliders[1].value = front;
			texts[1].text = string.Format("Front Brake: {0:00}%", front * 100f);
			sliders[2].value = rear;
			texts[2].text = string.Format("Rear Brake: {0:00}%", rear * 100f);

			//suspensions
			sliders[3].value = newState.SuspensionFront;
			texts[3].text = string.Format("Front Suspension: {0:000}mm", newState.SuspensionFront * bikeDefinition.Maxfrontsus);
			sliders[4].value = newState.SuspensionRear;
			texts[4].text = string.Format("Rear Suspension: {0:000}mm", newState.SuspensionRear * bikeDefinition.Maxrearsus);

			//seat
			sliders[5].value = newState.SeatPosition;
			texts[5].text = string.Format("Seat: {0:000}mm", bikeDefinition.Maxseatpos - (newState.SeatPosition * bikeDefinition.Maxseatpos));

			for (int i = 0; i < numBars; i++)
			{
				UpdateBarColor(i);
			}
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Ensures that value is between min and max.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns>The value if its within the bounds, the nearest bound otherwise.</returns>
		private float ClampFloat(float value, float min, float max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		/// <summary>
		/// Changes the color of the bar if it reaches CRITICAL LEVELS.
		/// </summary>
		/// <param name="slider"></param>
		private void UpdateBarColor(int index)
		{
			float value = sliders[index].value;
			if (value < 0.75f)
				infills[index].color = color_base;
			else if (value < 0.9f)
				infills[index].color = color_75;
			else
				infills[index].color = color_90;
		}

		#endregion

	}
}