using UnityEngine;

namespace SmidtFramework.ControllerSystem
{
	/// <summary>
	/// Base class for any visual controller. Creation / deletion is managed by the ControllerMaster.
	/// </summary>
	public abstract class BaseController
	{
		/// <summary>
		/// Reference to the world object associated with the Controller.
		/// </summary>
		public GameObject WorldObject { get; set; }

		/// <summary>
		/// Returns the prefab path of the GameObject to create. Returns null if no object needs to be created.
		/// </summary>
		/// <returns></returns>
		public abstract string GetPrefabPath();

		/// <summary>
		/// Called when the Controller gets created just after loading and creating the GameObject prefab (if needed).
		/// </summary>
		public abstract void Init();

		/// <summary>
		/// Called just before the Controller and its WorldObject are deleted.
		/// </summary>
		public abstract void OnDelete();
	}

}
