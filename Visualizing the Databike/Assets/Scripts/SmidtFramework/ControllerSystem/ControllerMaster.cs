using SmidtFramework.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SmidtFramework.ControllerSystem
{
	/// <summary>
	/// Class for managing creation, deletion, enabling and disabling (for UIControllers) of different controllers. Singleton, access through the "instance" field. 
	/// </summary>
	public class ControllerMaster : MonoBehaviour
	{
		public static ControllerMaster instance;

		public Transform worldParent;

		private Dictionary<string, BaseController> activeControllers;

		#region unity_functions
		public void Awake()
		{
			instance = this;
			activeControllers = new Dictionary<string, BaseController>();

			RemoveAllUI();
		}

		public void Start()
		{
			CreateController("console");
			DisableUI("console");

			//read init controllers from CMRegistry
			foreach(string id in CMRegistry.createOnInit)
			{
				CreateController(id);
			}
		}

		public void OnApplicationQuit()
		{
			GetCurrentController<VisualizingTheDatabike.Controllers.SceneController>("scene")?.HandleApplicationShutdown();
		}
		#endregion

		#region public_functions
		/// <summary>
		/// Creates a new controller of id C_ID. If the controller is already active an InvalidOperationException is thrown.
		/// </summary>
		/// <param name="C_ID">ID of the controller to create.</param>
		/// <returns>Reference to the created controller.</returns>
		public BaseController CreateController(string C_ID)
		{
			if (IsControllerCreated(C_ID))
			{
				LSLogger.LogError("Cannot create controller " + C_ID + " as it's already active!");
				throw new InvalidOperationException("Cannot create controller " + C_ID + " as it's already active!");
			}

			//create controller
			BaseController newController = GetNewControllerOfID(C_ID, out int uiLayer);
			if (newController == null)
			{
				throw new ArgumentException("No controller with id " + C_ID + " registered!");
			}
			
			//create scene object
			if (newController.GetPrefabPath() != null)
			{
				GameObject newObject = Instantiate(Resources.Load<GameObject>(newController.GetPrefabPath()));
				if (newController is BaseUIController)
				{
					newObject.transform.SetParent(transform.GetChild(uiLayer), false);
				}
				else
				{
					newObject.transform.SetParent(worldParent, false);
				}
				newController.WorldObject = newObject;
			}

			//register + init
			activeControllers.Add(C_ID, newController);
			newController.Init();

			//call OnEnable function if ui controller as its shown immediately
			if (newController is BaseUIController)
			{
				(newController as BaseUIController).OnEnable();
			}

			return newController;
		}

		/// <summary>
		/// Creates a new controller of id C_ID. If the controller is already active an InvalidOperationException is thrown.
		/// </summary>
		/// <typeparam name="T">Type of the controller to be created.</typeparam>
		/// <param name="C_ID">ID of the controller to create.</param>
		/// <returns>Reference to the created controller.</returns>
		public T CreateController<T>(string C_ID) where T : BaseController
		{
			BaseController baseController = CreateController(C_ID);
			if (baseController == null)
				return null;
			else
				return baseController as T;
		}

		/// <summary>
		/// Returns true if the controller of id C_ID is currently active, false otherwise.
		/// </summary>
		/// <param name="C_ID">ID of the controller.</param>
		public bool IsControllerCreated(string C_ID)
		{
			return activeControllers.ContainsKey(C_ID);
		}

		/// <summary>
		/// Deletes the controller with id C_ID and its corresponding scene object (if it exists), calling the OnDelete function beforehand. If the controller is a BaseUIController disables it first. Throws an InvalidOperationException if called on an inactive controller.
		/// </summary>
		/// <param name="C_ID">ID of the controller.</param>
		public void DeleteController(string C_ID)
		{
			if (!IsControllerCreated(C_ID))
			{
				LSLogger.LogError("Cannot delete controller " + C_ID + " because it is currently inactive.");
				throw new InvalidOperationException("Cannot delete controller " + C_ID + " because it is currently inactive.");
			}

			BaseController controller = activeControllers[C_ID];

			//if UIController disable first
			if (controller is BaseUIController)
			{
				if (IsControllerEnabled(C_ID))
				{
					DisableUI(C_ID);
				}
			}

			controller.OnDelete();
			if (controller.WorldObject != null)
			{
				Destroy(controller.WorldObject);
			}
			activeControllers.Remove(C_ID);
		}

		/// <summary>
		/// Calls DeleteController on the controller with id C_ID if it is currently active, does nothing otherwise.
		/// </summary>
		/// <param name="C_ID">ID of the controller.</param>
		public void DeleteIfActive(string C_ID)
		{
			if (IsControllerCreated(C_ID))
				DeleteController(C_ID);
		}

		/// <summary>
		/// Returns the current controller with id C_ID or null if it's currently inactive.
		/// </summary>
		/// <param name="C_ID">ID of the controller to retrieve.</param>
		/// <returns>A reference to the controller or null.</returns>
		public BaseController GetCurrentController(string C_ID)
		{
			return IsControllerCreated(C_ID) ? activeControllers[C_ID] : null;
		}

		/// <summary>
		/// Returns the current controller with id C_ID or null if it's currently inactive. Returns the refernce as the provided type for easier use. Returns null if the controller does not match the type T.
		/// </summary>
		/// <typeparam name="T">Controller type to cast to.</typeparam>
		/// <param name="C_ID">ID of the controller to retrieve.</param>
		/// <returns>A reference cast to T of the controller or null.</returns>
		public T GetCurrentController<T>(string C_ID) where T : BaseController
		{
			if (IsControllerCreated(C_ID))
			{
				if (activeControllers[C_ID] is T)
				{
					return activeControllers[C_ID] as T;
				}
			}
			return null;
		}

		/// <summary>
		/// Enables the UI of the controller of id C_ID, executing the OnEnable function. Does nothing if the UI is already enabled. Throws an InvalidOperationException if called on an inactive controller. Throws an ArgumentException when called on a BaseController, not a BaseUIController.
		/// </summary>
		/// <param name="C_ID">ID of the controller to enable.</param>
		public void EnableUI(string C_ID)
		{
			if (!IsControllerCreated(C_ID))
			{
				LSLogger.LogError("UI enable on inactive controller " + C_ID);
				throw new InvalidOperationException("UI enable on inactive controller " + C_ID);
			}

			BaseController controller = activeControllers[C_ID];
			if (controller is BaseUIController)
			{
				BaseUIController UIController = controller as BaseUIController;
				if (!UIController.WorldObject.activeSelf)
				{
					UIController.OnEnable();
					UIController.WorldObject.SetActive(true);
				}
			}
			else
			{
				LSLogger.LogError("UI enable call on controller " + C_ID + ", but controller is not UIController!");
				throw new ArgumentException("UI enable call on controller " + C_ID + ", but controller is not UIController!");
			}
		}

		/// <summary>
		/// Disables the UI of the controller of id C_ID, executing the OnDisable function. Does nothing if the UI is already disabled. Throws an InvalidOperationException if called on an inactive controller. Throws an ArgumentException when called on a BaseController, not a BaseUIController.
		/// </summary>
		/// <param name="C_ID">ID of the controller to disable.</param>
		public void DisableUI(string C_ID)
		{
			if (!IsControllerCreated(C_ID))
			{
				LSLogger.LogError("UI disable on inactive controller " + C_ID);
				throw new InvalidOperationException("UI disable on inactive controller " + C_ID);
			}

			BaseController controller = activeControllers[C_ID];
			if (controller is BaseUIController)
			{
				BaseUIController UIController = controller as BaseUIController;
				if (UIController.WorldObject.activeSelf)
				{
					UIController.OnDisable();
					UIController.WorldObject.SetActive(false);
				}
			}
			else
			{
				LSLogger.LogError("UI disable call on controller " + C_ID + ", but controller is not UIController!");
				throw new ArgumentException("UI disable call on controller " + C_ID + ", but controller is not UIController!");
			}
		}

		/// <summary>
		/// Switches the enabled state of the UI of the controller with id C_ID. Uses the EnableUI / DisableUI methods internally.
		/// </summary>
		/// <param name="C_ID">ID of the controller to switch the enabled state on.</param>
		public void SwitchUI(string C_ID)
		{
			if (IsControllerEnabled(C_ID))
			{
				DisableUI(C_ID);
			}
			else
			{
				EnableUI(C_ID);
			}
		}

		/// <summary>
		/// Checks if the controller with id C_ID is currently enabled or disabled. Only works on BaseUIControllers, always returns false for BaseControllers.
		/// </summary>
		/// <param name="C_ID">ID of the controller to check for.</param>
		/// <returns>True if the controller is currently enabled, false otherwise or on error.</returns>
		public bool IsControllerEnabled(string C_ID)
		{
			if (!IsControllerCreated(C_ID))
			{
				return false;
			}

			BaseController controller = activeControllers[C_ID];
			if (controller is BaseUIController)
			{
				BaseUIController UIController = controller as BaseUIController;
				if (UIController.WorldObject.activeSelf)
				{
					return true;
				}
			}

			return false;
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Creates and returns a new instace of a controller of id C_ID. Delegates the call to the CMRegistry function if the ID is not found in the base controllers.
		/// </summary>
		/// <param name="C_ID"></param>
		/// <param name="uiLayer">UI layer for the (potential) ui to be drawn on. [0-9], default: 4</param>
		/// <returns></returns>
		private BaseController GetNewControllerOfID(string C_ID, out int uiLayer)
		{
			switch (C_ID)
			{
				case "console":
					uiLayer = 9;
					return new LoggingSystem.ConsoleController();
				default:
					BaseController result = CMRegistry.GetNewControllerOfID(C_ID, out uiLayer);

					if (result == null)
						LSLogger.LogError("Unknown controller ID '" + C_ID + "'.");

					if (uiLayer < 0 || uiLayer > 9)
						uiLayer = 4;

					return result;
			}
		}

		/// <summary>
		/// Removes all UI elements. Called on startup.
		/// </summary>
		private void RemoveAllUI()
		{
			for (int j = 0; j <= 9; j++)
			{
				Transform panel = transform.GetChild(j);
				for (int i = panel.childCount - 1; i >= 0; i--)
				{
					Destroy(panel.GetChild(i).gameObject);
				}
			}
		}
		#endregion

	}
}

