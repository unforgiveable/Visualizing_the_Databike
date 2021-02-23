using SmidtFramework.ControllerSystem;
using SmidtFramework.LoggingSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.Controllers;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Service for applying, saving and loading settings configuration.
	/// </summary>
	public static class SettingsService
	{
		public const float UIScaleMin = 0.5f;
		public const float UIScaleMax = 2f;
		public static Settings CurrentSettings { get; set; }

		private static readonly string configFilePath = @"settings.xml";
		private static List<string> _displayResolutions;

		#region public_functions
		/// <summary>
		/// Applies the provided settings to the application and scene.
		/// </summary>
		public static void ApplySettings(Settings settings)
		{
			//apply application settings
			FullScreenMode fsMode = (FullScreenMode)(settings.FullscreenMode == 2 ? 3 : settings.FullscreenMode);
			string[] splitRes = settings.Resolution.Split('x');
			if (splitRes.Length != 2)
			{
				throw new ArgumentException("Settings resolution in invalid format. '" + settings.Resolution + "'");
			}
			Screen.SetResolution(Convert.ToInt32(splitRes[0]), Convert.ToInt32(splitRes[1]), fsMode);

			QualitySettings.vSyncCount = 0;
			int target = settings.FPSLimit == "off" ? 0 : Convert.ToInt32(settings.FPSLimit);
			Application.targetFrameRate = target;

			//set UI scale
			ControllerMaster.instance.gameObject.GetComponent<CanvasScaler>().scaleFactor = settings.UIScale;

			//enable / disable UI controllers
			ControllerMaster cmInstance = ControllerMaster.instance;
			void SwitchIfNecessary(string id, bool wantedState)
			{
				if (cmInstance.IsControllerCreated(id))
				{
					if (cmInstance.IsControllerEnabled(id) != wantedState)
						cmInstance.SwitchUI(id);
				}
			}
			SwitchIfNecessary("status_bar", settings.ShowStatusBars);
			SwitchIfNecessary("gear_indicator", settings.ShowGearsIndicator);
			SwitchIfNecessary("steering_indicator", settings.ShowSteeringIndicator);
			SwitchIfNecessary("artificial_horizon", settings.ShowArtificialHorizon);
			SwitchIfNecessary("compass", settings.ShowCompass);

			SceneController sceneController = cmInstance.GetCurrentController<SceneController>("scene");

			//recompute samples with new pedal offset and update the scene
			if (settings.PedalOffset == -1)
				GlobalValues.PedalOffset = sceneController.TimelinePedalOffset;
			else
				GlobalValues.PedalOffset = settings.PedalOffset;

			sceneController.PlaybackSystem?.UpdateAtCurrentTime();
		}

		/// <summary>
		/// Applies the current settings to the application and scene.
		/// </summary>
		public static void ApplyCurrentSettings()
		{
			ApplySettings(CurrentSettings);
		}

		/// <summary>
		/// Loads the settings from the config file. Loads default values if the file does not exist or is corrupted.
		/// </summary>
		public static void LoadSettingsFromFile()
		{
			if (!File.Exists(configFilePath))
			{
				LSLogger.Log("SettingsService", "No config file found, using default.");
				RestoreDefaultSettings();
				return;
			}

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Settings));
				using (FileStream fs = new FileStream(configFilePath, FileMode.Open))
				{
					CurrentSettings = (Settings) serializer.Deserialize(fs);

					if (CurrentSettings.UIScale < UIScaleMin || CurrentSettings.UIScale > UIScaleMax)
						throw new InvalidDataException("Loaded UI scale is out of bounds.");
				}
			}
			catch(Exception e)
			{
				LSLogger.Log("SettingsService", "Error while reading config file - " + e.ToString() + " - " + e.Message);
				RestoreDefaultSettings();
			}
		}

		/// <summary>
		/// Saves the current settings to the config file.
		/// </summary>
		public static void SaveSettingsToFile()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			TextWriter writer = new StreamWriter(configFilePath, false);
			serializer.Serialize(writer, CurrentSettings);
			writer.Close();
		}

		/// <summary>
		/// Returns a list of all supported fullscreen display resolutions sorted from smallest to largest horizontal resolutions.
		/// </summary>
		public static List<string> GetDisplayResolutions()
		{
			if (_displayResolutions == null)
				_displayResolutions = Screen.resolutions.OrderBy(x => x.width).Select(x => x.width + "x" + x.height).Distinct().ToList();

			return _displayResolutions;
		}

		/// <summary>
		/// Returns an array of all FPS limit options.
		/// </summary>
		public static string[] GetFPSLimitOptions()
		{
			return new string[] { "30", "60", "90", "120", "off" };
		}
		#endregion

		/// <summary>
		/// Sets the current settings to the default and writes the default values to the config file.
		/// </summary>
		private static void RestoreDefaultSettings()
		{
			Settings defaultSettings = new Settings()
			{
				FullscreenMode = 1,
				FPSLimit = "60",
				UIScale = 1f,
				PedalOffset = -1,
				ShowStatusBars = true,
				ShowGearsIndicator = true,
				ShowSteeringIndicator = true,
				ShowArtificialHorizon = true,
				ShowCompass = true
			};
			//use default native resolution
			List<string> resolutions = GetDisplayResolutions();
			defaultSettings.Resolution = resolutions[resolutions.Count-1];

			CurrentSettings = defaultSettings;
			SaveSettingsToFile();
		}
	}
}
