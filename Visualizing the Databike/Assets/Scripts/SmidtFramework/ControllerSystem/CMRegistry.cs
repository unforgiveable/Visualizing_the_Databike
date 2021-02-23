using VisualizingTheDatabike.Controllers;

namespace SmidtFramework.ControllerSystem
{
	/// <summary>
	/// Class for specifying project-specific constants used by the ControllerMaster.
	/// </summary>
	internal static class CMRegistry
	{
		/// <summary>
		/// All controllers with the ids specified here are created at the start of the application.
		/// </summary>
		public static string[] createOnInit = new string[] { "scene" };

		/// <summary>
		/// Lookup method for associating controller ids with the respective classes. All controllers to be used with the ControllerMaster must be specified here.
		/// </summary>
		/// <param name="C_ID">ID of the controller. Must be unique.</param>
		/// <param name="uiLayer">UI layer to be used if the controller is of type BaseUIController. In range of 0-9.</param>
		/// <returns></returns>
		public static BaseController GetNewControllerOfID(string C_ID, out int uiLayer)
		{
			uiLayer = 4;
			switch (C_ID)
			{
				case "scene":
					return new SceneController();
				case "bikeanimation":
					return new BikeAnimationController();
				case "playback":
					uiLayer = 5;
					return new PlaybackController();
				case "status_bar":
					return new BarController();
				case "camera":
					uiLayer = 0;
					return new CameraController();
				case "trail":
					return new TrailController();
				case "onscreen_controls":
					uiLayer = 5;
					return new OnScreenControlsController();
				case "steering_indicator":
					return new SteeringIndicatorController();
				case "artificial_horizon":
					return new ArtificialHorizonController();
				case "gear_indicator":
					return new GearsIndicatorController();
				case "escape_menu":
					uiLayer = 6;
					return new EscapeMenuController();
				case "settings_menu":
					uiLayer = 7;
					return new SettingsMenuController();
				case "main_menu":
					return new MainMenuController();
				case "timeline_menu":
					uiLayer = 5;
					return new TimelineMenuController();
				case "universal_dialogue":
					uiLayer = 8;
					return new UniversalDialogueController();
				case "compass":
					return new CompassController();
				case "help_overlay":
					uiLayer = 5;
					return new HelpOverlayController();
				default:
					return null;
			}
		}
	}
}
