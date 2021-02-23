using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Class for managing the main menu UI. ID "main_menu".
	/// </summary>
	public class MainMenuController : BaseUIController
	{
		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/MainMenu";
		}

		public override void Init()
		{
			ControllerMaster CMinstance = ControllerMaster.instance;
			//load timeline button
			WorldObject.transform.GetChild(0).GetChild(1).GetComponent<ButtonScript>().ClickAction = () =>
			{
				CMinstance.EnableUI("timeline_menu");
				CMinstance.DisableUI("main_menu");
			};

			//settings button
			WorldObject.transform.GetChild(0).GetChild(2).GetComponent<ButtonScript>().ClickAction = () =>
			{
				CMinstance.EnableUI("settings_menu");
			};

			//quit to OS button
			WorldObject.transform.GetChild(0).GetChild(3).GetComponent<ButtonScript>().ClickAction = () => QuitButtonAction();

			//version text
			WorldObject.transform.GetChild(1).GetComponent<Text>().text = "Version " + Application.version;
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
			InputSystem.instance.RemoveKeyAction(KeyCode.Escape, "main_menu_escape");
		}

		public override void OnEnable()
		{
			InputSystem.instance.AddKeyAction("main_menu_escape", KeyCode.Escape, InputSystem.KeyPressType.onDown, () => QuitButtonAction());
		}
		#endregion

		#region private_functions
		private void QuitButtonAction()
		{
			UniversalDialogueController universalDialogueController = ControllerMaster.instance.CreateController<UniversalDialogueController>("universal_dialogue");
			universalDialogueController.SetupWindow("Are you sure you want to quit?", "No", "Yes", null, () =>
			{
				Application.Quit();
			});
		}
		#endregion
	}
}
