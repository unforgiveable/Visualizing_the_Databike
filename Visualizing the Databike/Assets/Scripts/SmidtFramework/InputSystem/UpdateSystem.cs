using SmidtFramework.Utility;
using SmidtFramework.LoggingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmidtFramework.InputSystem
{
	/// <summary>
	/// System for dynamically managing functions to be called within the Update / FixedUpdate functions.
	/// </summary>
	public class UpdateSystem : MonoBehaviour
	{
		#region fields
		public static UpdateSystem instance;

		private List<Pair<string, Action>> updateActions;
		private List<Pair<string, Action>> fixedUpdateActions;

		private List<Pair<string, Action>> updateAdditionBuffer;
		private List<string> updateRemovalBuffer;

		private List<Pair<string, Action>> fixedUpdateAdditionBuffer;
		private List<string> fixedUpdateRemovalBuffer;
		#endregion

		#region unity_functions
		public void Awake()
		{
			instance = this;
			updateActions = new List<Pair<string, Action>>();
			fixedUpdateActions = new List<Pair<string, Action>>();

			updateAdditionBuffer = new List<Pair<string, Action>>();
			updateRemovalBuffer = new List<string>();

			fixedUpdateAdditionBuffer = new List<Pair<string, Action>>();
			fixedUpdateRemovalBuffer = new List<string>();
		}

		public void Update() 
		{
			//invoke registered functions
			foreach (var a in updateActions)
			{
				a.Second();
			}
		}

		public void LateUpdate()
		{
			//manage additions and removals of actions
			if (updateAdditionBuffer.Count > 0)
			{
				HashSet<string> existing = new HashSet<string>(updateActions.Select(x => x.First));
				foreach (var a in updateAdditionBuffer)
				{
					if (existing.Contains(a.First))
					{
						LSLogger.Log("UpdateSystem", "Unable to add update action with identifier '" + a.First + "', entry already exists!");
						continue;
					}
					updateActions.Add(a);
				}
				updateAdditionBuffer.Clear();
			}

			if (updateRemovalBuffer.Count > 0)
			{
				HashSet<string> removals = new HashSet<string>(updateRemovalBuffer);
				for (int i = updateActions.Count - 1; i >= 0; i--)
				{
					if (removals.Contains(updateActions[i].First))
					{
						updateActions.RemoveAt(i);
					}
				}
				updateRemovalBuffer.Clear();
			}
		}

		public void FixedUpdate()
		{
			//invoke registered functions
			foreach (var a in fixedUpdateActions)
			{
				a.Second();
			}

			//manage additions and removals of actions
			//no separate unity method for after fixed update - execute after invokation of registered functions instead
			if (fixedUpdateAdditionBuffer.Count > 0)
			{
				HashSet<string> existing = new HashSet<string>(fixedUpdateActions.Select(x => x.First));
				foreach (var a in fixedUpdateAdditionBuffer)
				{
					if (existing.Contains(a.First))
					{
						LSLogger.Log("UpdateSystem", "Unable to add fixed update action with identifier '" + a.First + "', entry already exists!");
						continue;
					}
					fixedUpdateActions.Add(a);
				}
				fixedUpdateAdditionBuffer.Clear();
			}

			if (fixedUpdateRemovalBuffer.Count > 0)
			{
				HashSet<string> removals = new HashSet<string>(fixedUpdateRemovalBuffer);
				for (int i = fixedUpdateActions.Count - 1; i >= 0; i--)
				{
					if (removals.Contains(fixedUpdateActions[i].First))
					{
						fixedUpdateActions.RemoveAt(i);
					}
				}
				fixedUpdateRemovalBuffer.Clear();
			}
		}
		#endregion

		#region public_functions
		/// <summary>
		/// Adds the action with id to the list of actions to execute inside the Update call.
		/// </summary>
		/// <param name="id">unique id of the action</param>
		/// <param name="action">function to be invoked</param>
		public void AddUpdateAction(string id, Action action)
		{
			if (string.IsNullOrEmpty(id) || action == null)
			{
				throw new ArgumentException("Invalid arguments for adding update action.");
			}
			
			updateAdditionBuffer.Add(new Pair<string, Action>(id, action));
		}

		/// <summary>
		/// Removes the action with id from the list of actions to exectue inside the Update call.
		/// </summary>
		/// <param name="id">id of the action</param>
		public void RemoveUpdateAction(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentException("Invalid argument for removing update action.");
			}

			updateRemovalBuffer.Add(id);
		}

		/// <summary>
		/// Adds the action with id to the list of actions to execute inside the FixedUpdate call.
		/// </summary>
		/// <param name="id">unique id of the action</param>
		/// <param name="action">function to be invoked</param>
		public void AddFixedUpdateAction(string id, Action action)
		{
			if (string.IsNullOrEmpty(id) || action == null)
			{
				throw new ArgumentException("Invalid arguments for adding fixed update action.");
			}

			fixedUpdateAdditionBuffer.Add(new Pair<string, Action>(id, action));
		}

		/// <summary>
		/// Removes the action with id from the list of actions to exectue inside the FixedUpdate call.
		/// </summary>
		/// <param name="id">id of the action</param>
		public void RemoveFixedUpdateAction(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentException("Invalid argument for removing fixed update action.");
			}

			fixedUpdateRemovalBuffer.Add(id);
		}
		#endregion

	}
}