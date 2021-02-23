using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualizingTheDatabike.Utility
{
	/// <summary>
	/// Script to allow for runtime configuration of functions to be called by UI elements. Allows for multiple different event functions on the same GameObject.
	/// </summary>
	public class EventScript : MonoBehaviour
	{
		public int EventCount = 1;

		private Action[] EventActions;

		public void ExecuteEvent(int index)
		{
			EventActions[index]?.Invoke();
		}

		public void SetEvent(int index, Action eventAction)
		{
			if (index < 0 || index >= EventCount) throw new ArgumentOutOfRangeException("Invalid index for setting event action.");

			if (EventActions == null) EventActions = new Action[EventCount];
			EventActions[index] = eventAction;
		}

	}
}
