using SmidtFramework.ControllerSystem;
using UnityEngine;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for animating the current bike.
	/// Interacts with the Unity Animator attached to the bike. ID "bikeanimation".
	/// </summary>
	public class BikeAnimationController : BaseController, IBikePlaybackVisualizer
	{
		private Animator animator;

		#region controller_overrides
		public override string GetPrefabPath()
		{
			//get bike prefab path from SceneController
			var sc = ControllerMaster.instance.GetCurrentController<SceneController>("scene");
			if (sc == null) return null;

			return sc.BikeDefinition.Prefabpath;
		}

		public override void Init()
		{
			animator = WorldObject.GetComponent<Animator>();

			animator.speed = 1;
		}

		public override void OnDelete()
		{
		}
		#endregion

		public void UpdateWithNewBikeState(BikeState newState)
		{
			//world position / rotation
			WorldObject.transform.position = newState.Position;
			WorldObject.transform.rotation = Quaternion.Euler(newState.Rotation);

			//normalized motions
			animator.Play("Front_suspension", 0, newState.SuspensionFront);
			animator.Play("Rear_suspension", 1, newState.SuspensionRear);
			animator.Play("Saddle", 2, newState.SeatPosition);
			animator.Play("Brake_left", 3, newState.BrakeLeft);
			animator.Play("Brake_right", 4, newState.BrakeRight);

			//rotations
			float steeringNormalized = (newState.SteeringRotation + 360f) % 360f / 360f; //map from [-180 - 180] to [0-1]
			animator.Play("Steering_rotation", 5, steeringNormalized);

			float pedalsNormalized = (newState.PedalRotation + newState.Pedaloffset) % 360f;
			if (pedalsNormalized < 0) pedalsNormalized += 360f;
			pedalsNormalized /= 360f;
			animator.Play("Pedals", 6, pedalsNormalized);

			//wheels
			animator.SetFloat("wheel_speed", newState.WheelRPM / 30f);
		}

		public void UpdatePlaybackState(bool newState)
		{
			if (!newState) //pause wheels on pause - are automatically restarted on play
			{
				animator.SetFloat("wheel_speed", 0f);
			}
		}
	}
}
