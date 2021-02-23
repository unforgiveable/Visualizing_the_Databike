using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.LoggingSystem;
using UnityEngine;
using VisualizingTheDatabike.DataModel;
using VisualizingTheDatabike.Utility;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for managing the main camera. Handles the setup of the UI element for capturing the mouse dragging. Adjusts the camera position (relative to the bike) in 3D space. ID "camera".
	/// </summary>
	public class CameraController : BaseUIController, IBikeVisualizer
	{
		public enum CameraMode
		{
			undefined,
			orbit,
			free
		}

		#region fields
		//constant values
		private Vector3 focusOffset = new Vector3(0, 0.75f, 0);
		private const float pixelsPerDegree = 10f; //sets camera rotation speed
		private const float baseCameraDistance = 2f; //(orbit) starting distance of camera from bike
		private const float cameraMoveSpeed = 5f;
		private const float cameraZoomSpeed = 1f; //(orbit)
		private const float orbitCamMinDistance = 0.5f;
		private const float orbitCamMaxDistance = 10f;
		private const float oribtCamPolarDeadzone = 5f; //deadzone around top/bottom in degrees

		//cached references
		private Transform mainCamera;
		private Transform bikeObject;

		//runtime values
		private CameraMode currentMode = CameraMode.undefined;
		private Vector3 lastMousePos; //used to track mouse drag when rotating the camera
		private Vector3 baseCameraOffset; //(orbit) base offset of the camera from the bike, sets the distance
		private Vector3 currentCameraOffset; //(orbit) current camera offset, used for efficient moving of the camera with the bike
		private float currentCameraDistance; //(orbit) current distance of the camera from the bike
		private Vector2 cameraAngles; //current camera rotation angles
		#endregion

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/CameraControlUI";
		}

		public override void Init()
		{
			//find references
			mainCamera = Camera.main.transform;
			bikeObject = ControllerMaster.instance.GetCurrentController<BikeAnimationController>("bikeanimation").WorldObject.transform;

			mainCamera.SetParent(null);

			//setup UI element events
			EventScript eventScript = WorldObject.GetComponent<EventScript>();
			eventScript.SetEvent(0, () =>
			{
				BeginUIDrag();
			});

			eventScript.SetEvent(1, () =>
			{
				EndUIDrag();
			});

			SwitchModeToOrbit();
		}

		/// <summary>
		/// CameraController gets deleted when switching from Timeline view / world view to main menu
		/// </summary>
		public override void OnDelete()
		{
			//Remove all possibly active control functions
			InputSystem.instance.RemoveKeyAction(KeyCode.W, "camera_move_forward");
			InputSystem.instance.RemoveKeyAction(KeyCode.A, "camera_move_left");
			InputSystem.instance.RemoveKeyAction(KeyCode.D, "camera_move_right");
			InputSystem.instance.RemoveKeyAction(KeyCode.S, "camera_move_back");

			UpdateSystem.instance.RemoveUpdateAction("camera_zoom");

			mainCamera.position = Vector3.zero;
			mainCamera.rotation = Quaternion.Euler(Vector3.zero);
		}

		public override void OnEnable()
		{
		}

		public override void OnDisable()
		{
		}
		#endregion


		#region public_functions
		/// <summary>
		/// Changes the camera control mode to orbit around the bike.
		/// </summary>
		public void SwitchModeToOrbit()
		{
			if (currentMode == CameraMode.orbit) return;

			//remove free movement controls
			if (currentMode == CameraMode.free)
			{
				InputSystem.instance.RemoveKeyAction(KeyCode.W, "camera_move_forward");
				InputSystem.instance.RemoveKeyAction(KeyCode.A, "camera_move_left");
				InputSystem.instance.RemoveKeyAction(KeyCode.D, "camera_move_right");
				InputSystem.instance.RemoveKeyAction(KeyCode.S, "camera_move_back");
			}

			currentMode = CameraMode.orbit;

			baseCameraOffset = new Vector3(-baseCameraDistance, 0, 0);
			currentCameraDistance = baseCameraDistance;
			cameraAngles = new Vector2(0, 0);
			mainCamera.position = bikeObject.position + baseCameraOffset + focusOffset;
			currentCameraOffset = baseCameraOffset;
			mainCamera.LookAt(bikeObject.position + focusOffset);

			//add camera zoom controls
			UpdateSystem.instance.AddUpdateAction("camera_zoom", () =>
			{
				float scrollValue = Input.GetAxis("Mouse ScrollWheel");

				if (Mathf.Abs(scrollValue) > 0)
				{
					currentCameraDistance -= scrollValue * cameraZoomSpeed;
					if (currentCameraDistance < orbitCamMinDistance) currentCameraDistance = orbitCamMinDistance;
					if (currentCameraDistance > orbitCamMaxDistance) currentCameraDistance = orbitCamMaxDistance;

					baseCameraOffset = new Vector3(-currentCameraDistance, 0, 0);
					UpdateOrbitCameraPosition();
				}
			});
		}

		/// <summary>
		/// Changes the camera control mode to move freely within the scene.
		/// </summary>
		public void SwitchModeToFree()
		{
			if (currentMode == CameraMode.free) return;

			//remove zoom controls
			if (currentMode == CameraMode.orbit)
			{
				UpdateSystem.instance.RemoveUpdateAction("camera_zoom");
			}

			currentMode = CameraMode.free;
			cameraAngles = new Vector2(0, 0);
			mainCamera.rotation = Quaternion.Euler(new Vector3(-cameraAngles.x, cameraAngles.y, 0));

			//add movement controls
			InputSystem.instance.AddKeyAction("camera_move_forward", KeyCode.W, InputSystem.KeyPressType.onIsPressed, () =>
			{
				mainCamera.position += mainCamera.forward * Time.deltaTime * cameraMoveSpeed;
			});
			InputSystem.instance.AddKeyAction("camera_move_left", KeyCode.A, InputSystem.KeyPressType.onIsPressed, () =>
			{
				mainCamera.position += (-mainCamera.right) * Time.deltaTime * cameraMoveSpeed;
			});
			InputSystem.instance.AddKeyAction("camera_move_right", KeyCode.D, InputSystem.KeyPressType.onIsPressed, () =>
			{
				mainCamera.position += mainCamera.right * Time.deltaTime * cameraMoveSpeed;
			});
			InputSystem.instance.AddKeyAction("camera_move_back", KeyCode.S, InputSystem.KeyPressType.onIsPressed, () =>
			{
				mainCamera.position += (-mainCamera.forward) * Time.deltaTime * cameraMoveSpeed;
			});
		}

		/// <summary>
		/// Updates the camera position relative to the bike model if the orbit camera mode is selected.
		/// </summary>
		/// <param name="newState"></param>
		public void UpdateWithNewBikeState(BikeState newState)
		{
			if (currentMode == CameraMode.orbit)
				mainCamera.position = newState.Position + currentCameraOffset + focusOffset;
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Called when the user starts to drag on the screen. Creates the camera position update function and submits it to the update system.
		/// </summary>
		private void BeginUIDrag()
		{
			lastMousePos = Input.mousePosition;

			switch (currentMode)
			{
				case CameraMode.orbit:
					UpdateSystem.instance.AddUpdateAction("camera_rotation", () =>
					{
						UpdateCameraAngels();

						UpdateOrbitCameraPosition();
					});
					break;

				case CameraMode.free:
					UpdateSystem.instance.AddUpdateAction("camera_rotation", () =>
					{
						UpdateCameraAngels();

						mainCamera.rotation = Quaternion.Euler(new Vector3(-cameraAngles.x, cameraAngles.y, 0));
					});
					break;

				default:
					LSLogger.LogError("Invalid camera mode selected on UI drag start.");
					break;
			}
		}

		/// <summary>
		/// Computes the currentCameraOffset based on the current camera Angles and baseCameraOffset, and updates the cameras position. Used only in orbit rotation mode.
		/// </summary>
		private void UpdateOrbitCameraPosition()
		{
			//rotate base camera offset around the current angles and update camera position
			currentCameraOffset = Quaternion.Euler(new Vector3(0, cameraAngles.y, cameraAngles.x)) * baseCameraOffset;
			mainCamera.position = bikeObject.position + currentCameraOffset + focusOffset;
			mainCamera.LookAt(bikeObject.position + focusOffset);
		}

		/// <summary>
		/// Updates the current camera rotation angles from the mouse movement position change relative to the last frame.
		/// </summary>
		private void UpdateCameraAngels()
		{
			const float maxAngle = 90f - oribtCamPolarDeadzone;

			//get mouse movement
			Vector3 currentMousePos = Input.mousePosition;
			float rot_hor = (currentMousePos.x - lastMousePos.x) / pixelsPerDegree;
			float rot_vert = (currentMousePos.y - lastMousePos.y) / pixelsPerDegree;

			//add to current camera rotation
			cameraAngles.x += rot_vert;
			cameraAngles.y += rot_hor;

			if (cameraAngles.x > maxAngle)
			{
				cameraAngles.x = maxAngle;
			}
			else if (cameraAngles.x < -maxAngle)
			{
				cameraAngles.x = -maxAngle;
			}

			cameraAngles.y %= 360;

			lastMousePos = currentMousePos;
		}

		/// <summary>
		/// Called when the user stops dragging on the screen. Removes the camera position update function from the update system.
		/// </summary>
		private void EndUIDrag()
		{
			UpdateSystem.instance.RemoveUpdateAction("camera_rotation");
		}
		#endregion


	}
}