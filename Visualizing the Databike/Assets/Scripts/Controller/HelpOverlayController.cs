using SmidtFramework.ControllerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the Help Overlay. ID "help_overlay".
	/// </summary>
	public class HelpOverlayController : BaseUIController
	{
		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/HelpOverlay";
		}

		public override void Init()
		{
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
		}
		#endregion
	}
}
