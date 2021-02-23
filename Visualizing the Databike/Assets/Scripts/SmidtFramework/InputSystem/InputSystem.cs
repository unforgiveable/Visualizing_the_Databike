using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SmidtFramework.Utility;

namespace SmidtFramework.InputSystem
{
	/// <summary>
	/// System for dynamically managing keyboard inputs.
	/// </summary>
	public class InputSystem : MonoBehaviour
	{
		#region type_definitions
		/// <summary>
		/// Struct describing a keybind, containing an unique identifier, a PressType and the Action to be called when the key is pressed.
		/// </summary>
		private struct KeyAction
		{
			public string Identifier { get; set; }
			public KeyPressType Type { get; set; }
			public Action OnPressAction { get; set; }

			public override bool Equals(object obj)
			{
				if (!(obj is KeyAction))
				{
					return false;
				}
					
				var action = (KeyAction)obj;
				return Identifier == action.Identifier;
			}

			public override int GetHashCode()
			{
				return 1186239758 + EqualityComparer<string>.Default.GetHashCode(Identifier);
			}
		}

		/// <summary>
		/// Struct describing an input lock, containing an unique identifier and a set of exceptions in form of KeyAction identifiers.
		/// </summary>
		private struct InputLock
		{
			public string Identifier { get; set; }
			public HashSet<string> Exceptions { get; set; }

			public override bool Equals(object obj)
			{
				return obj is InputLock @lock &&
						Identifier == @lock.Identifier;
			}

			public override int GetHashCode()
			{
				return 1186239758 + EqualityComparer<string>.Default.GetHashCode(Identifier);
			}
		}

		public enum KeyPressType
		{
			onDown,
			onIsPressed,
			onUp
		}
		#endregion

		#region fields
		public static InputSystem instance;

		private Dictionary<KeyCode, List<KeyAction>> keyActions;

		// buffers to delay changes of the dictionary until end of update
		private List<Pair<KeyCode, KeyAction>> additionBuffer = new List<Pair<KeyCode, KeyAction>>();
		private List<Pair<KeyCode, string>> removeBuffer = new List<Pair<KeyCode, string>>();

		private List<InputLock> inputLocks; //list of currently registered InputLocks
		#endregion

		#region unity_event_functions
		public void Awake()
		{
			keyActions = new Dictionary<KeyCode, List<KeyAction>>();
			inputLocks = new List<InputLock>();
			instance = this;
		}

		//check for any inputs
		public void Update()
		{
			foreach (KeyValuePair<KeyCode, List<KeyAction>> pair in keyActions)
			{
				foreach (KeyAction action in pair.Value)
				{
					if (inputLocks.Count > 0 && !inputLocks[inputLocks.Count - 1].Exceptions.Contains(action.Identifier))
						continue;

					switch (action.Type)
					{
						case KeyPressType.onDown:
							if (Input.GetKeyDown(pair.Key))
							{
								action.OnPressAction();
							}
							break;
						case KeyPressType.onUp:
							if (Input.GetKeyUp(pair.Key))
							{
								action.OnPressAction();
							}
							break;
						case KeyPressType.onIsPressed:
							if (Input.GetKey(pair.Key))
							{
								action.OnPressAction();
							}
							break;
					}
				}
			}
		}

		public void LateUpdate()
		{
			//apply buffers
			if (additionBuffer.Count > 0)
			{
				foreach (Pair<KeyCode, KeyAction> pair in additionBuffer)
				{
					if (!keyActions.ContainsKey(pair.First))
					{
						keyActions.Add(pair.First, new List<KeyAction>());
					}
					keyActions[pair.First].Add(pair.Second);
				}
				additionBuffer.Clear();
			}
			if (removeBuffer.Count > 0)
			{
				foreach (Pair<KeyCode, string> pair in removeBuffer)
				{
					if (!keyActions.ContainsKey(pair.First)) continue;

					keyActions[pair.First].Remove(new KeyAction() { Identifier = pair.Second });

					if (keyActions[pair.First].Count == 0)
						keyActions.Remove(pair.First);
				}
				removeBuffer.Clear();
			}

		}
		#endregion

		#region public_functions
		/// <summary>
		/// Adds a new Input Lock to the system. The lock prevents all KeyActions from being triggered except the ones specified in the exceptions. The new lock is only in affect if it is the most recent one in the system (stack-based). The identifier must be unique and not already registered, otherwise an ArgumentException is thrown.
		/// </summary>
		/// <param name="identifier">Unique identifier of the lock.</param>
		/// <param name="exceptions">List of KeyAction identifers to allow during the lock.</param>
		public void AddInputLock(string identifier, string[] exceptions = null)
		{
			if (inputLocks.Select(x => x.Identifier).Contains(identifier))
			{
				LoggingSystem.LSLogger.LogError("Attempting to register Input Lock with identifier already in use! ('" + identifier + "')");
				throw new ArgumentException("Attempting to register Input Lock with identifier already in use!");
			}

			InputLock newLock = new InputLock()
			{
				Identifier = identifier,
				Exceptions = new HashSet<string>(exceptions)
			};
			inputLocks.Add(newLock);
		}

		/// <summary>
		/// Removes an Input Lock from the system. This is effective immediately. Does nothing if the lock does not exist.
		/// </summary>
		/// <param name="identifier">Identifier of the lock to remove.</param>
		public void RemoveInputLock(string identifier)
		{
			for (int i = 0; i < inputLocks.Count; i++)
			{
				if (inputLocks[i].Identifier == identifier)
				{
					inputLocks.RemoveAt(i);
					return;
				}
			}
		}

		/// <summary>
		/// Adds action of identifier to be called when the KeyCode of key is pressed. Addition is buffered until after the update event.
		/// </summary>
		/// <param name="identifier">Identifier for the key action to be registered. If an action with the same identifier is already registered the new one is discarded.</param>
		/// <param name="key">Keycode for the action.</param>
		/// <param name="type">Press type of the action.</param>
		/// <param name="action">Action to be executed when the key is pressed.</param>
		public void AddKeyAction(string identifier, KeyCode key, KeyPressType type, Action action)
		{
			if (identifier == null || action == null)
			{
				LoggingSystem.LSLogger.LogError("Missing arguments for adding key action.");
				throw new ArgumentException("Missing arguments for adding key action.");
			}

			additionBuffer.Add(new Pair<KeyCode, KeyAction>(key, new KeyAction() { Identifier = identifier, Type = type, OnPressAction = action }));
		}

		/// <summary>
		/// Removes the action with the identifier for KeyCode key. Does nothing if no action with the specified KeyCode and identifier is in the system. Removal is buffered until after the update event.
		/// </summary>
		/// <param name="key">Keycode of the action to be removed.</param>
		/// <param name="identifier">Identifier of the action to be removed.</param>
		public void RemoveKeyAction(KeyCode key, string identifier)
		{
			removeBuffer.Add(new Pair<KeyCode, string>(key, identifier));
		}

		/// <summary>
		/// Checks if a key action for the key with identifier is currently registered.
		/// </summary>
		/// <param name="key">Keycode to be checked.</param>
		/// <param name="identifier">Identifier to be checked.</param>
		/// <returns><see langword="true"/> if the KeyCode and identifier combination is currently registered, <see langword="false"/> otherwise.</returns>
		public bool IsKeyActionRegistered(KeyCode key, string identifier)
		{
			if (!keyActions.ContainsKey(key))
				return false;

			return keyActions[key].Select(x => x.Identifier).Contains(identifier);
		}
		#endregion
	}
}

