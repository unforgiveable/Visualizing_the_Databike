using SmidtFramework.ControllerSystem;
using SmidtFramework.LoggingSystem;
using System.Collections.Generic;
using System.IO;
using VisualizingTheDatabike.DataModel;
using VisualizingTheDatabike.Services;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for managing the scene. Gets created at application start, handles all UI and world configuration changes. ID "scene".
	/// </summary>
	public class SceneController : BaseController, IBikePlaybackVisualizer
	{
		#region fields
		private InterpolatedTimeline interpolatedTimeline;

		public float TimelinePedalOffset { get; private set; }
		public BikeDefinition BikeDefinition { get; private set; }
		public PlaybackSystem PlaybackSystem { get; private set; }

		//cached references to controllers
		private List<IBikeVisualizer> bikeVisualizers = new List<IBikeVisualizer>();
		private List<IBikePlaybackVisualizer> bikePlaybackVisualizers = new List<IBikePlaybackVisualizer>();

		private int numberOfTimestamps = -1;
		#endregion

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return null;
		}

		public override void Init()
		{
			LSLogger.Log("SceneController", "Application start.");

			try
			{
				SettingsService.LoadSettingsFromFile();
				SettingsService.ApplyCurrentSettings();
			}
			catch (System.Exception e)
			{
				LSLogger.LogError("Error loading / applying settings at startup: " + e.ToString());
			}

			ControllerMaster.instance.CreateController("settings_menu");
			ControllerMaster.instance.DisableUI("settings_menu");

			SwitchConfigToMainMenu();
		}

		public override void OnDelete()
		{
		}
		#endregion

		#region public_functions
		/// <summary>
		/// Called by the Menu UI when a timeline is selected for loading. Sets up the BikeDefinition and PlaybackSystem if successful. Throws an ArgumentException if the file is not found, InvalidDataException otherwise.
		/// </summary>
		/// <param name="timelinePath">Absolute path to the timeline file.</param>
		public void LoadTimeline(string timelinePath)
		{
			if (string.IsNullOrEmpty(timelinePath))
			{
				LSLogger.LogError("Invalid timeline path - null or empty");
				throw new System.ArgumentException("Timeline path is null or empty.");
			}

			//read timeline from file
			ITimelineReader reader = new XMLTimelineReader();
			BikeDefinition bikeDefinition;
			RawTimeline rawTimeline;
			try
			{
				rawTimeline = reader.ReadTimelineFromFile(timelinePath, out bikeDefinition);
			}
			catch (InvalidDataException e)
			{
				LSLogger.LogError("Error reading timeline from file '" + timelinePath + "': " + e.Message);
				throw new InvalidDataException("Error reading timeline from file", e);
			}
			BikeDefinition = bikeDefinition;
			numberOfTimestamps = rawTimeline.PosX.Count;

			//convert raw to interpolated timeline
			interpolatedTimeline = TimelineConverter.GetInterpolatedFromRawTimeline(rawTimeline);

			//create playback system
			PlaybackSystem = new PlaybackSystem(interpolatedTimeline);

			//save timeline offset
			TimelinePedalOffset = interpolatedTimeline.Pedaloffset;
			if (SettingsService.CurrentSettings.PedalOffset == -1)
				GlobalValues.PedalOffset = TimelinePedalOffset;
		}

		/// <summary>
		/// Propagates the updated BikeState to all controllers requiring the new state.
		/// </summary>
		/// <param name="newState">The new Bike state.</param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			foreach (IBikeVisualizer bikeVisualizer in bikeVisualizers)
			{
				bikeVisualizer.UpdateWithNewBikeState(newState);
			}
			foreach (IBikePlaybackVisualizer bikePlaybackVisualizer in bikePlaybackVisualizers)
			{
				bikePlaybackVisualizer.UpdateWithNewBikeState(newState);
			}
		}

		/// <summary>
		/// Notifies the appropriate controllers of a change in the current playback state (i.e. play / pause)
		/// </summary>
		public void UpdatePlaybackState(bool newState)
		{
			foreach (IBikePlaybackVisualizer bikePlaybackVisualizer in bikePlaybackVisualizers)
			{
				bikePlaybackVisualizer.UpdatePlaybackState(newState);
			}
		}

		/// <summary>
		/// Called on application quit, calls all controllers / systems that need to perform cleanup.
		/// </summary>
		public void HandleApplicationShutdown()
		{
			LSLogger.Log("SceneController", "Application quit.");

			ControllerMaster.instance.GetCurrentController<ConsoleController>("console").CloseLogFileHandle();
		}
		#endregion

		#region scene_configurations
		/// <summary>
		/// Removes all main scene controllers if necessary and creates the main menu controller.
		/// </summary>
		public void SwitchConfigToMainMenu()
		{
			//remove main world controllers if necessary
			ControllerMaster CMInstance = ControllerMaster.instance;

			CMInstance.DeleteIfActive("bikeanimation");
			CMInstance.DeleteIfActive("playback");
			CMInstance.DeleteIfActive("camera");
			CMInstance.DeleteIfActive("trail");
			CMInstance.DeleteIfActive("onscreen_controls");
			CMInstance.DeleteIfActive("help_overlay");
			CMInstance.DeleteIfActive("status_bar");
			CMInstance.DeleteIfActive("steering_indicator");
			CMInstance.DeleteIfActive("artificial_horizon");
			CMInstance.DeleteIfActive("gear_indicator");
			CMInstance.DeleteIfActive("escape_menu");
			CMInstance.DeleteIfActive("compass");

			//reset data structures
			bikeVisualizers.Clear();
			bikePlaybackVisualizers.Clear();
			PlaybackSystem = null;
			BikeDefinition = null;

			//create main menu controller
			CMInstance.CreateController("main_menu");
			CMInstance.CreateController("timeline_menu");
			CMInstance.DisableUI("timeline_menu");
		}

		/// <summary>
		/// Creates all controllers required for the main world scene. Removes the main menu and timeline menu controllers if necessary.
		/// </summary>
		public void SwitchConfigToMainWorld()
		{
			ControllerMaster cmInstance = ControllerMaster.instance;
			Settings currentSettings = SettingsService.CurrentSettings;

			//remove main menu + timeline loading controllers
			cmInstance.DeleteIfActive("main_menu");
			cmInstance.DeleteIfActive("timeline_menu");

			//create controllers
			bikePlaybackVisualizers.Add(cmInstance.CreateController<BikeAnimationController>("bikeanimation"));
			bikePlaybackVisualizers.Add(cmInstance.CreateController<PlaybackController>("playback"));
			bikeVisualizers.Add(cmInstance.CreateController<CameraController>("camera"));
			cmInstance.CreateController("trail");
			cmInstance.CreateController("onscreen_controls");
			cmInstance.CreateController("help_overlay");
			cmInstance.DisableUI("help_overlay");

			bikeVisualizers.Add(cmInstance.CreateController<BarController>("status_bar"));
			if (!currentSettings.ShowStatusBars) cmInstance.DisableUI("status_bar");

			bikeVisualizers.Add(cmInstance.CreateController<SteeringIndicatorController>("steering_indicator"));
			if (!currentSettings.ShowSteeringIndicator) cmInstance.DisableUI("steering_indicator");

			bikeVisualizers.Add(cmInstance.CreateController<ArtificialHorizonController>("artificial_horizon"));
			if (!currentSettings.ShowArtificialHorizon) cmInstance.DisableUI("artificial_horizon");

			bikeVisualizers.Add(cmInstance.CreateController<GearsIndicatorController>("gear_indicator"));
			if (!currentSettings.ShowGearsIndicator) cmInstance.DisableUI("gear_indicator");

			bikeVisualizers.Add(cmInstance.CreateController<CompassController>("compass"));
			if (!currentSettings.ShowCompass) cmInstance.DisableUI("compass");

			//setup escape menu
			EscapeMenuController escapeMenuController = cmInstance.CreateController<EscapeMenuController>("escape_menu");
			cmInstance.DisableUI("escape_menu");
			escapeMenuController.SetupEscapeMenuStats(new string[]
			{
				interpolatedTimeline.Name,
				interpolatedTimeline.Bikename,
				FormatUtility.GetTimeStringFromSeconds(interpolatedTimeline.Length),
				numberOfTimestamps.ToString()
			});

			//update scene with intitial bike state
			PlaybackSystem.SetCurrentTime(0f);
			PlaybackSystem.ForcePausePlayback();
		}
		#endregion

	}
}
