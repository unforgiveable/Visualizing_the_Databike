using SmidtFramework.ControllerSystem;
using UnityEngine;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the bike position trail. ID "trail".
	/// </summary>
	public class TrailController : BaseController
	{
		private const int samplesPerSecond = 4;
		private Color32 trailColor = new Color32(200, 200, 200, 255);

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/WorldObjects/Trail";
		}

		public override void Init()
		{
			LineRenderer line = WorldObject.GetComponent<LineRenderer>();
			SceneController sceneController = ControllerMaster.instance.GetCurrentController<SceneController>("scene");
			Vector3[] trailPoints = sceneController.PlaybackSystem.GetTrailSamples(samplesPerSecond);

			line.positionCount = trailPoints.Length;
			line.SetPositions(trailPoints);
			line.startColor = trailColor;
			line.endColor = trailColor;

			//LSLogger.Log("TrailController", "Created trail with " + trailPoints.Length + " samples.");
		}

		public override void OnDelete()
		{
		}
		#endregion


	}
}