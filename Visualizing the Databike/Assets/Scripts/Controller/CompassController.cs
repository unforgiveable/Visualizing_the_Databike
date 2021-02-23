using SmidtFramework.ControllerSystem;
using UnityEngine;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the compass UI. ID "compass".
	/// </summary>
	public class CompassController : BaseUIController, IBikeVisualizer
	{
		private const float rot180RightOffset = 213.38f;

		private RectTransform textPanel;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/Compass";
		}

		public override void Init()
		{
			textPanel = WorldObject.transform.GetChild(0).GetComponent<RectTransform>();

			textPanel.anchoredPosition = new Vector2(0, -2);
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

		public void UpdateWithNewBikeState(BikeState newState)
		{
			float rotation = (((newState.Rotation.y % 360f) + 360f) % 360f) - 180f;
			float offset = (rotation / 180f) * rot180RightOffset;

			textPanel.anchoredPosition = new Vector2(- offset, -2);
		}
	}
}
