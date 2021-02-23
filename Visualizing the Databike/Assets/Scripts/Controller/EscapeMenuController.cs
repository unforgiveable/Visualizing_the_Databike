using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.LoggingSystem;
using SmidtFramework.Utility;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.Services;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the escape menu. ID "escape_menu".
	/// </summary>
	public class EscapeMenuController : BaseUIController
	{
		private PlaybackSystem playbackSystem;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/EscapeMenu";
		}

		public override void Init()
		{
			//get reference to current PlaybackSystem for pausing playback on open
			playbackSystem = ControllerMaster.instance.GetCurrentController<SceneController>("scene").PlaybackSystem;

			//create menu toggle keybind
			InputSystem.instance.AddKeyAction("escape_menu_toggle", KeyCode.Escape, InputSystem.KeyPressType.onDown, () =>
			{
				ControllerMaster.instance.SwitchUI("escape_menu");
			});

			//setup buttons
			Transform buttonsParent = WorldObject.transform.GetChild(0);
			//settings button
			buttonsParent.GetChild(2).GetComponent<ButtonScript>().ClickAction = () =>
			{
				ControllerMaster.instance.EnableUI("settings_menu");
			};
			//quit to menu button
			buttonsParent.GetChild(3).GetComponent<ButtonScript>().ClickAction = () =>
			{
				UniversalDialogueController universalDialogueController = ControllerMaster.instance.CreateController<UniversalDialogueController>("universal_dialogue");
				universalDialogueController.SetupWindow("Are you sure you want to quit to the main menu?", "No", "Yes", null, () =>
				{
					ControllerMaster.instance.GetCurrentController<SceneController>("scene").SwitchConfigToMainMenu();
				});
			};
			//quit to desktop button
			buttonsParent.GetChild(4).GetComponent<ButtonScript>().ClickAction = () =>
			{
				UniversalDialogueController universalDialogueController = ControllerMaster.instance.CreateController<UniversalDialogueController>("universal_dialogue");
				universalDialogueController.SetupWindow("Are you sure you want to quit to the OS?", "No", "Yes", null, () =>
				{
					Application.Quit();
				});
			};
			//back button
			buttonsParent.GetChild(5).GetComponent<ButtonScript>().ClickAction = () =>
			{
				ControllerMaster.instance.DisableUI("escape_menu");
			};

		}

		public override void OnDelete()
		{
			//remove menu toggle keybind
			InputSystem.instance.RemoveKeyAction(KeyCode.Escape, "escape_menu_toggle");
		}

		public override void OnDisable()
		{
			//re-allow other key presses after closing
			InputSystem.instance.RemoveInputLock("escape_menu_open_lock");
		}

		public override void OnEnable()
		{
			//pause playback if currently running
			playbackSystem.PausePlayback();
			
			//prevent any other key presses from being registered while escape menu is open
			InputSystem.instance.AddInputLock("escape_menu_open_lock", new string[] { "escape_menu_toggle", "consoleToggle" });
		}
		#endregion


		/// <summary>
		/// Sets the escape menu stats to the specified values. Each item in the array corresponds to the stat line in that order. Called from the SceneManager during scene config switch.
		/// </summary>
		/// <param name="values"></param>
		public void SetupEscapeMenuStats(string[] values)
		{
			Transform valuesParent = WorldObject.transform.GetChild(0).GetChild(1).GetChild(1);
			if (values.Length != valuesParent.childCount)
			{
				LSLogger.LogError("EscapeMenu stats setup provided values array length does not match line count.");
				return;
			}

			for (int i = 0; i < values.Length; i++)
			{
				valuesParent.GetChild(i).GetComponent<Text>().text = values[i];
			}
		}

	}
}
