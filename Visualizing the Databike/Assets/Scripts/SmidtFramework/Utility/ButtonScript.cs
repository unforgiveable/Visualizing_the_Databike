using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmidtFramework
{
	namespace Utility
	{
		public class ButtonScript : MonoBehaviour
		{
			public Action ClickAction { get; set; }

			public void Click()
			{
				ClickAction?.Invoke();
			}
		}
	}
}

