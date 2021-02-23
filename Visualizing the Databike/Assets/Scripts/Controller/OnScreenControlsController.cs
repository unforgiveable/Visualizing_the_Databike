using SmidtFramework.ControllerSystem;
using SmidtFramework.LoggingSystem;
using SmidtFramework.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the on-screen control buttons. ID "onscreen_controls".
	/// </summary>
	public class OnScreenControlsController : BaseUIController
	{
		//cached values
		private Sprite imageCamModeOrbit;
		private Sprite imageCamModeFree;
		private CameraController cameraController;
		private Image cameraToggleButtonImage;

		private CameraController.CameraMode currentMode = CameraController.CameraMode.orbit;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/OnScreenControls";
		}

		public override void Init()
		{
			//pre-load button sprites
			imageCamModeOrbit = Resources.Load<Sprite>("UI/CameraButton_orbit");
			imageCamModeFree = Resources.Load<Sprite>("UI/CameraButton_free");
			cameraToggleButtonImage = WorldObject.transform.GetChild(1).GetComponent<Image>();

			//get reference
			cameraController = ControllerMaster.instance.GetCurrentController<CameraController>("camera");

			//setup escape menu button
			WorldObject.transform.GetChild(0).GetComponent<ButtonScript>().ClickAction = () =>
			{
				ControllerMaster.instance.EnableUI("escape_menu");
			};

			//setup camera mode toggle button
			WorldObject.transform.GetChild(1).GetComponent<ButtonScript>().ClickAction = () =>
			{
				switch (currentMode)
				{
					case CameraController.CameraMode.orbit:
						//switch to free mode, change image to orbit
						currentMode = CameraController.CameraMode.free;
						cameraToggleButtonImage.sprite = imageCamModeOrbit;
						cameraController.SwitchModeToFree();
						break;

					case CameraController.CameraMode.free:
						//switch to orbit mode, change image to free
						currentMode = CameraController.CameraMode.orbit;
						cameraToggleButtonImage.sprite = imageCamModeFree;
						cameraController.SwitchModeToOrbit();
						break;

					default:
						LSLogger.LogError("OnScreenControls invalid current camera mode.");
						break;
				}
			};

			//setup help overlay button
			WorldObject.transform.GetChild(2).GetComponent<ButtonScript>().ClickAction = () =>
			{
				ControllerMaster.instance.SwitchUI("help_overlay");
			};

			//set startup state
			cameraToggleButtonImage.sprite = imageCamModeFree;
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
	}
}
