using System.Text;
using UnityEngine;

namespace SmidtFramework.LoggingSystem
{
	/// <summary>
	/// Class for logging debug information to the console.
	/// </summary>
	public static class LSLogger
	{
		//set to false to prevent output to the unity debug logger.
		private static readonly bool MirrorToEditor = true;

		private static StringBuilder builder = new StringBuilder();

		#region public_functions
		/// <summary>
		/// Logs the message to the console. Origin specifies the source system of the message.
		/// </summary>
		/// <param name="origin">Origin of the message, usually class name.</param>
		/// <param name="message">Message to be output.</param>
		public static void Log(string origin, string message)
		{
			builder.Clear();
			builder.Append("[");
			builder.Append(origin);
			builder.Append("] ");
			builder.Append(message);

			ConsoleController.CurrentConsole?.Write(builder.ToString());
			if (MirrorToEditor)
			{
				Debug.Log(builder.ToString());
			}
		}

		/// <summary>
		/// Logs the message as an error to the console.
		/// </summary>
		/// <param name="message">Message to be output.</param>
		public static void LogError(string message)
		{
			Debug.LogError(message);
			ConsoleController.CurrentConsole?.Write("<color=red>[ERROR]</color> " + message);
		}
		#endregion

	}
}

