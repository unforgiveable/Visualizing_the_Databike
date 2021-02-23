using SmidtFramework.ControllerSystem;
using UnityEngine;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the artificial horizon UI. ID "artificial_horizon".
	/// </summary>
	public class ArtificialHorizonController : BaseUIController, IBikeVisualizer
	{
		private RectTransform horizonTransform;
		private RectTransform dialTransform;


		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/ArtificialHorzion";
		}

		public override void Init()
		{
			horizonTransform = WorldObject.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
			dialTransform = WorldObject.transform.GetChild(2).GetComponent<RectTransform>();
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
		/// Updates the artifical horizon to match the new bike rotation.
		/// </summary>
		/// <param name="newState"></param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			//tilt dial rotation
			float dialRotation = newState.Rotation.z % 360f;
			if (dialRotation < -90f) dialRotation = -90f;
			else if (dialRotation > 90f) dialRotation = 90f;

			dialTransform.rotation = Quaternion.Euler(0, 0, dialRotation);

			//horizon rotation based on tilt
			horizonTransform.rotation = Quaternion.Euler(0, 0, dialRotation);

			//clamp vertical rotation to fit onto dial - loop around from top to bottom
			float vertRotation = newState.Rotation.x % 180;
			if (vertRotation > 90) vertRotation -= 180;
			else if (vertRotation < -90) vertRotation += 180;

			//calculate offset of horizon panel
			Vector2 horizonPosition = new Vector2();
			float tiltRotationDeg = dialRotation * (Mathf.PI / 180);
			float vertOffset = (-257.2f / 90f) * vertRotation; //base vertical offset in pixels
			horizonPosition.y = Mathf.Cos(tiltRotationDeg) * vertOffset; //vertical offset for rotation
			horizonPosition.x = - Mathf.Sin(tiltRotationDeg) * vertOffset; //horizontal compensation for vertical rotation

			horizonTransform.anchoredPosition = horizonPosition;
		}
	}
}
