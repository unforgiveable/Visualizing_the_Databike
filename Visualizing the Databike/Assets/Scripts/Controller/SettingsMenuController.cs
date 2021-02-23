using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.LoggingSystem;
using SmidtFramework.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.DataModel;
using VisualizingTheDatabike.Services;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the settings menu. ID "settings_menu".
	/// </summary>
	public class SettingsMenuController : BaseUIController
	{
		private int tempDisplayMode;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/SettingsMenu";
		}

		public override void Init()
		{
			Transform leftPanel = WorldObject.transform.GetChild(0).GetChild(3);
			//display mode button
			leftPanel.GetChild(1).GetComponent<ButtonScript>().ClickAction = () =>
			{
				tempDisplayMode += 1;
				tempDisplayMode %= 3;
				UpdateDisplayButton(leftPanel);
			};

			//resolution dropdown
			Dropdown resolutionDD = leftPanel.GetChild(3).GetComponent<Dropdown>();
			resolutionDD.ClearOptions();
			foreach (string resText in SettingsService.GetDisplayResolutions())
			{
				resolutionDD.options.Add(new Dropdown.OptionData(resText));
			}

			//fps limit dropdown
			Dropdown fpsLimitDD = leftPanel.GetChild(5).GetComponent<Dropdown>();
			fpsLimitDD.ClearOptions();
			string[] limits = SettingsService.GetFPSLimitOptions();
			for (int i = 0; i < limits.Length; i++)
			{
				fpsLimitDD.options.Add(new Dropdown.OptionData(limits[i].ToString()));
			}

			//UI scale slider
			Slider uiScaleSlider = leftPanel.GetChild(7).GetComponent<Slider>();
			uiScaleSlider.minValue = (int) (SettingsService.UIScaleMin * 10);
			uiScaleSlider.maxValue = (int) (SettingsService.UIScaleMax * 10);
			Text uiScaleText = uiScaleSlider.transform.GetChild(0).GetComponent<Text>();
			uiScaleSlider.gameObject.GetComponent<ButtonScript>().ClickAction = () =>
			{
				uiScaleText.text = string.Format("{0:0.0}x", uiScaleSlider.value / 10f);
			};


			Transform rightPanel = WorldObject.transform.GetChild(0).GetChild(4);
			//pedal offset override inputfield
			InputField pedalOffsetInput = rightPanel.GetChild(1).GetComponent<InputField>();
			pedalOffsetInput.transform.GetComponent<ButtonScript>().ClickAction = () =>
			{
				string offsetText = pedalOffsetInput.text;
				if (string.IsNullOrEmpty(offsetText)) return;

				float offset = Convert.ToSingle(offsetText);
				if (offset > 360f) offset %= 360f;
				if (offset < 0f) offset = 0f;
				pedalOffsetInput.text = offset.ToString();
			};

			//cancel button - discard changes (indirectly) and close
			WorldObject.transform.GetChild(0).GetChild(5).GetComponent<ButtonScript>().ClickAction = () =>
			{
				SettingsService.ApplyCurrentSettings(); //restore previous settings
				ControllerMaster.instance.DisableUI("settings_menu");
			};

			//apply button - apply but no save
			WorldObject.transform.GetChild(0).GetChild(6).GetComponent<ButtonScript>().ClickAction = () =>
			{
				Settings tempSettings = ReadSettingsFromInterface();
				SettingsService.ApplySettings(tempSettings);
				UpdateInterface(tempSettings);
			};

			//save button - apply, save to file, and close
			WorldObject.transform.GetChild(0).GetChild(7).GetComponent<ButtonScript>().ClickAction = () =>
			{
				SettingsService.CurrentSettings = ReadSettingsFromInterface();
				SettingsService.ApplyCurrentSettings();
				SettingsService.SaveSettingsToFile();
				ControllerMaster.instance.DisableUI("settings_menu");
			};
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
			InputSystem.instance.RemoveKeyAction(KeyCode.Escape, "settings_menu_close");

			InputSystem.instance.RemoveInputLock("settings_menu_lock");
		}

		public override void OnEnable()
		{
			//make sure interface is up to date
			UpdateInterface(SettingsService.CurrentSettings);

			InputSystem.instance.AddKeyAction("settings_menu_close", KeyCode.Escape, InputSystem.KeyPressType.onDown, () =>
			{
				SettingsService.ApplyCurrentSettings(); //restore previous settings
				ControllerMaster.instance.DisableUI("settings_menu");
			});

			InputSystem.instance.AddInputLock("settings_menu_lock", new string[] { "settings_menu_close", "consoleToggle" });
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Updates the settings menu interface to match the current settings
		/// </summary>
		private void UpdateInterface(Settings settings)
		{
			Transform leftPanel = WorldObject.transform.GetChild(0).GetChild(3);
			//display mode
			tempDisplayMode = settings.FullscreenMode;
			UpdateDisplayButton(leftPanel);

			//resolution dropdown
			Dropdown resolutionDD = leftPanel.GetChild(3).GetComponent<Dropdown>();
			if (tempDisplayMode != 2)
			{
				resolutionDD.interactable = true;
				int selectedRes = -1;
				for (int i = 0; i < resolutionDD.options.Count; i++)
				{
					if (settings.Resolution == resolutionDD.options[i].text) selectedRes = i;
				}

				if (selectedRes == -1)
				{
					LSLogger.Log("SettingsMenuController", "Saved fullscreen resolution incompatible with monitor - choosing other.");
					selectedRes = resolutionDD.options.Count - 1;
				}

				resolutionDD.value = selectedRes;
			}
			else
			{
				resolutionDD.interactable = false; //disable in windowed mode
				resolutionDD.value = resolutionDD.options.Count - 1;
			}

			//fps limit dropdown
			Dropdown fpsLimitDD = leftPanel.GetChild(5).GetComponent<Dropdown>();
			int selectedLimit = -1;
			for (int i = 0; i < fpsLimitDD.options.Count; i++)
			{
				if (settings.FPSLimit == fpsLimitDD.options[i].text) selectedLimit = i;
			}
			if (selectedLimit == -1) LSLogger.LogError("Saved fps limit not in list.");
			else fpsLimitDD.value = selectedLimit;

			//UI scale slider
			leftPanel.GetChild(7).GetChild(0).GetComponent<Text>().text = string.Format("{0:0.0}x", settings.UIScale);
			leftPanel.GetChild(7).GetComponent<Slider>().value = (int) (settings.UIScale * 10);

			Transform rightPanel = WorldObject.transform.GetChild(0).GetChild(4);
			//pedal offset
			rightPanel.GetChild(1).GetComponent<InputField>().text = settings.PedalOffset != -1 ? settings.PedalOffset.ToString() : "";

			//status bar toggle
			rightPanel.GetChild(4).GetComponent<Toggle>().isOn = settings.ShowStatusBars;

			//gears indicator toggle
			rightPanel.GetChild(6).GetComponent<Toggle>().isOn = settings.ShowGearsIndicator;

			//steering indicator toggle
			rightPanel.GetChild(8).GetComponent<Toggle>().isOn = settings.ShowSteeringIndicator;

			//artificial horizon toggle
			rightPanel.GetChild(10).GetComponent<Toggle>().isOn = settings.ShowArtificialHorizon;

			//compass toggle
			rightPanel.GetChild(12).GetComponent<Toggle>().isOn = settings.ShowCompass;
		}

		/// <summary>
		/// Updates the text on the display mode button
		/// </summary>
		/// <param name="leftPanel"></param>
		private void UpdateDisplayButton(Transform leftPanel)
		{
			Text buttonText = leftPanel.GetChild(1).GetChild(0).GetComponent<Text>();

			switch (tempDisplayMode)
			{
				case 0:
					buttonText.text = "Fullscreen";
					break;
				case 1:
					buttonText.text = "Borderless";
					break;
				case 2:
					buttonText.text = "Windowed";
					break;
			}
		}

		/// <summary>
		/// Reads the values form the interface elements and updates the SettingsService.CurrentSettings with the new values.
		/// </summary>
		private Settings ReadSettingsFromInterface()
		{
			Settings newSettings = new Settings();

			Transform leftPanel = WorldObject.transform.GetChild(0).GetChild(3);
			//display mode
			newSettings.FullscreenMode = tempDisplayMode;

			//resolution
			if (tempDisplayMode != 2)
			{
				Dropdown resolutionDD = leftPanel.GetChild(3).GetComponent<Dropdown>();
				newSettings.Resolution = resolutionDD.options[resolutionDD.value].text;
			}
			else
			{
				newSettings.Resolution = Screen.width + "x" + Screen.height;
			}

			//fps limit
			Dropdown fpsLimitDD = leftPanel.GetChild(5).GetComponent<Dropdown>();
			newSettings.FPSLimit = fpsLimitDD.options[fpsLimitDD.value].text;

			//UI scale
			newSettings.UIScale = leftPanel.GetChild(7).GetComponent<Slider>().value / 10f;

			Transform rightPanel = WorldObject.transform.GetChild(0).GetChild(4);
			//pedal offset
			string offsetText = rightPanel.GetChild(1).GetComponent<InputField>().text;
			if (string.IsNullOrEmpty(offsetText))
			{
				newSettings.PedalOffset = -1;
			}
			else
			{
				float offset = Convert.ToSingle(offsetText);
				if (offset > 360f) offset %= 360f;
				if (offset < 0f) offset = 0f;
				newSettings.PedalOffset = offset;
			}

			//status bar toggle
			newSettings.ShowStatusBars = rightPanel.GetChild(4).GetComponent<Toggle>().isOn;

			//gears indicator toggle
			newSettings.ShowGearsIndicator = rightPanel.GetChild(6).GetComponent<Toggle>().isOn;

			//steering indicator toggle
			newSettings.ShowSteeringIndicator = rightPanel.GetChild(8).GetComponent<Toggle>().isOn;

			//artificial horizon toggle
			newSettings.ShowArtificialHorizon = rightPanel.GetChild(10).GetComponent<Toggle>().isOn;

			//compass toggle
			newSettings.ShowCompass = rightPanel.GetChild(12).GetComponent<Toggle>().isOn;

			return newSettings;
		}
		#endregion
	}
}
