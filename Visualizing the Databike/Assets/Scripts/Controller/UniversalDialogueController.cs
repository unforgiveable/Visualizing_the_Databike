using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the universal dialogue window. ID "universal_dialogue".
	/// </summary>
	public class UniversalDialogueController : BaseUIController
	{
		private Transform mainPanel;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/UniversalDialogue";
		}

		public override void Init()
		{
			mainPanel = WorldObject.transform.GetChild(0);
		}

		public override void OnDelete()
		{
			InputSystem.instance.RemoveKeyAction(KeyCode.Escape, "dialogue_single_option_escape");
			InputSystem.instance.RemoveInputLock("dialogue_window_lock");
		}

		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
		}
		#endregion

		#region public_functions
		/// <summary>
		/// Sets up the dialogue window with one button. The button is automatically set up to delete the dialogue window on click.
		/// </summary>
		/// <param name="message">Message to be shown.</param>
		/// <param name="option">Text of the button.</param>
		/// <param name="action">Action to be executed on button press.</param>
		public void SetupWindow(string message, string option, Action action)
		{
			SetMessage(message);
			mainPanel.GetChild(1).gameObject.SetActive(false);
			mainPanel.GetChild(2).gameObject.SetActive(false);

			SetupButton(mainPanel.GetChild(3), option, action);

			InputSystem.instance.AddKeyAction("dialogue_single_option_escape", KeyCode.Escape, InputSystem.KeyPressType.onDown, () =>
			{
				action?.Invoke();
				ControllerMaster.instance.DeleteIfActive("universal_dialogue");
			});

			InputSystem.instance.AddInputLock("dialogue_window_lock", new string[] { "dialogue_single_option_escape", "consoleToggle" });
		}

		/// <summary>
		/// Sets up the dialogue window with two buttons. The buttons are automatically set up to delete the dialogue window on click.
		/// </summary>
		/// <param name="message">Message to be shown.</param>
		/// <param name="option1">Text of the left button.</param>
		/// <param name="option2">Text of the right button.</param>
		/// <param name="action1">Action to be executed on left button press.</param>
		/// <param name="action2">Action to be executed on right button press.</param>
		public void SetupWindow(string message, string option1, string option2, Action action1, Action action2)
		{
			SetMessage(message);

			SetupButton(mainPanel.GetChild(1), option1, action1);
			SetupButton(mainPanel.GetChild(2), option2, action2);

			mainPanel.GetChild(3).gameObject.SetActive(false);

			InputSystem.instance.AddInputLock("dialogue_window_lock", new string[] { "consoleToggle" });
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Sets the message of the dialogue window.
		/// </summary>
		/// <param name="message"></param>
		private void SetMessage(string message)
		{
			mainPanel.GetChild(0).GetComponent<Text>().text = message;
		}

		/// <summary>
		/// Enables and sets up the button with the option as text and the action as click action.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="option"></param>
		/// <param name="action"></param>
		private void SetupButton(Transform transform, string option, Action action)
		{
			transform.gameObject.SetActive(true);
			transform.GetChild(0).GetComponent<Text>().text = option;
			transform.GetComponent<ButtonScript>().ClickAction = () =>
			{
				action?.Invoke();
				ControllerMaster.instance.DeleteIfActive("universal_dialogue");
			};
		}
		#endregion
	}
}
