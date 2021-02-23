using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmidtFramework.ControllerSystem;
using System.Linq;
using SmidtFramework.Utility;
using SmidtFramework.InputSystem;

namespace SmidtFramework.LoggingSystem
{
	/// <summary>
	/// Controller for managing the console UI. Uses the ConsoleAPI for the console itself. ID "console".
	/// </summary>
	public class ConsoleController : BaseUIController
	{
		public static readonly string ID = "console";
		public static Console CurrentConsole;

		private int LastRecallIndex = -1;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/Console";
		}

		public override void Init()
		{
			//create new console
			CurrentConsole = new Console(WorldObject.transform.GetChild(0).GetChild(0).GetComponent<Text>(), 12, 10);
			CurrentConsole.Write("Console startup complete");

			//setup input field OnEndEdit action
			InputField _inputField = WorldObject.transform.GetChild(2).GetComponent<InputField>();
			WorldObject.transform.GetChild(2).GetComponent<ButtonScript>().ClickAction = () =>
			{
				if (!(_inputField.text == ""))
				{
					try
					{
						CurrentConsole.ExecuteCommand(_inputField.text);
					}
					catch (System.Exception e)
					{
						CurrentConsole.Write("<color=red>Error in last command execution - " + e.ToString() + "</color>");
					}
					_inputField.text = "";
					_inputField.ActivateInputField();
					LastRecallIndex = -1;
				}
			};

			//register keybind to toggle console window
			InputSystem.InputSystem.instance.AddKeyAction("consoleToggle", KeyCode.F9, InputSystem.InputSystem.KeyPressType.onDown, () =>
			{
				ControllerMaster.instance.SwitchUI("console");
			});

			//add commands to console
			AddDefaultCommands();

			List<Console.ConsoleCommand> commands = ConsoleCommandRegistry.GetCustomCommands();
			foreach(Console.ConsoleCommand command in commands)
			{
				CurrentConsole.AddCommand(command);
			}
		}

		public override void OnDelete()
		{
			//remove keybinds
			InputSystem.InputSystem IS = InputSystem.InputSystem.instance;
			IS.RemoveKeyAction(KeyCode.F9, "consoleToggle");

			IS.RemoveKeyAction(KeyCode.UpArrow, "consoleRecallUp");
			IS.RemoveKeyAction(KeyCode.DownArrow, "consoleRecallDown");
			IS.RemoveKeyAction(KeyCode.Tab, "consoleAutoComplete");

			CurrentConsole = null;
		}

		public override void OnDisable()
		{
			//reset input field
			InputField _inputField = WorldObject.transform.GetChild(2).GetComponent<InputField>();
			_inputField.text = "";
			_inputField.DeactivateInputField();

			InputSystem.InputSystem IS = InputSystem.InputSystem.instance;
			IS.RemoveInputLock("console");

			//remove console keys
			IS.RemoveKeyAction(KeyCode.UpArrow, "consoleRecallUp");
			IS.RemoveKeyAction(KeyCode.DownArrow, "consoleRecallDown");
			IS.RemoveKeyAction(KeyCode.Tab, "consoleAutoComplete");

		}

		public override void OnEnable()
		{
			WorldObject.transform.GetChild(1).GetComponent<Scrollbar>().value = 0;
			InputField _inputField = WorldObject.transform.GetChild(2).GetComponent<InputField>();

			//select the inputfield in the next Update call
			UpdateSystem.instance.AddUpdateAction("consoleInputSelect", () =>
			{
				_inputField.Select();
				_inputField.ActivateInputField();
				UpdateSystem.instance.RemoveUpdateAction("consoleInputSelect");
			});

			//setup for input recall
			InputSystem.InputSystem IS = InputSystem.InputSystem.instance;
			IS.AddKeyAction("consoleRecallUp", KeyCode.UpArrow, InputSystem.InputSystem.KeyPressType.onDown, () =>
			{
				List<string> lastInputs = CurrentConsole.GetLastInputs();
				if (LastRecallIndex < lastInputs.Count - 1)
					LastRecallIndex++;

				_inputField.text = (LastRecallIndex == -1) ? "" : lastInputs[LastRecallIndex];
			});

			IS.AddKeyAction("consoleRecallDown", KeyCode.DownArrow, InputSystem.InputSystem.KeyPressType.onDown, () =>
			{
				List<string> lastInputs = CurrentConsole.GetLastInputs();
				if (LastRecallIndex > -1)
					LastRecallIndex--;

				_inputField.text = (LastRecallIndex == -1) ? "" : lastInputs[LastRecallIndex];
			});

			//setup auto-completion
			IS.AddKeyAction("consoleAutoComplete", KeyCode.Tab, InputSystem.InputSystem.KeyPressType.onDown, () =>
			{
				var matches = CurrentConsole.GetMatchingCommands(_inputField.text);
				string common = (matches.Count > 0) ? matches[0] : _inputField.text;
				foreach (string m in matches)
				{
					for (int i = 0; i < common.Length; i++)
					{
						if (i >= m.Length || common[i] != m[i])
						{
							common = common.Remove(i);
							break;
						}
					}
				}
				_inputField.text = common;
				_inputField.caretPosition = common.Length;
			});

			//add input lock to prevent other keyboard inputs from registering
			IS.AddInputLock("console", new string[4] { "consoleToggle", "consoleRecallUp", "consoleRecallDown", "consoleAutoComplete" });
		}
		#endregion

		/// <summary>
		/// Called on application quit or on mirroring disable. Signals the console to close the logfile handle.
		/// </summary>
		public void CloseLogFileHandle()
		{
			CurrentConsole.CloseLogFileHandle();
		}

		/// <summary>
		/// Adds some default QOL commands to the console.
		/// </summary>
		private void AddDefaultCommands()
		{
			CurrentConsole.AddCommand(new Console.ConsoleCommand("clear", (x) =>
			{
				WorldObject.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "";
			}));

			CurrentConsole.AddCommand(new Console.ConsoleCommand("list", (x) =>
			{
				CurrentConsole.Write("Available Commands:\n"+CurrentConsole.GetMatchingCommands("").Aggregate((y,z) => y + "\n"+ z));
			}));
		}

	}

}

