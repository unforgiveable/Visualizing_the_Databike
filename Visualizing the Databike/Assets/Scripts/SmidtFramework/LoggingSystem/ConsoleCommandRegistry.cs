using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VisualizingTheDatabike.Controllers;
using VisualizingTheDatabike.Services;

namespace SmidtFramework.LoggingSystem
{
	/// <summary>
	/// Class for specifying project-specific console commands.
	/// </summary>
	internal static class ConsoleCommandRegistry
	{
		/// <summary>
		/// Gets called from the ConsoleController at application start. All commands contained within the returned list are added to the console. Command identifiers must be unique. All exceptions thrown by the execution of the command are output to the console.
		/// </summary>
		/// <returns>A list of commands to be added to the console.</returns>
		public static List<Console.ConsoleCommand> GetCustomCommands()
		{
			List<Console.ConsoleCommand> consoleCommands = new List<Console.ConsoleCommand>();

			consoleCommands.Add(new Console.ConsoleCommand("testreadtimeline", (x) =>
			{
				bool prevValue = GlobalValues.DebugTimelineReader;
				GlobalValues.DebugTimelineReader = true;

				ITimelineReader reader = new XMLTimelineReader();
				reader.ReadTimelineFromFile(@"" + x[0], out VisualizingTheDatabike.DataModel.BikeDefinition bikeDefinition);

				GlobalValues.DebugTimelineReader = prevValue;
			}));

			consoleCommands.Add(new Console.ConsoleCommand("setcammode", (x) =>
			{
				if (x.Count != 1)
				{
					LSLogger.Log("SetCamMode", "Invalid argument count - usage: setcammode [free:orbit]");
					return;
				}

				if (x[0].ToLower() == "free")
					ControllerMaster.instance.GetCurrentController<CameraController>("camera").SwitchModeToFree();
				else if (x[0].ToLower() == "orbit")
					ControllerMaster.instance.GetCurrentController<CameraController>("camera").SwitchModeToOrbit();
				else
					LSLogger.Log("SetCamMode", "Invalid argument - usage: setcammode [free:orbit]");
			}));

			consoleCommands.Add(new Console.ConsoleCommand("debugsamplesystem", (x) =>
			{
				if (x.Count > 1)
				{
					LSLogger.Log("DebugSampleSystem", "Invalid argument count - usage: debugsamplesystem [true:extended:false]");
				}
				else if (x.Count == 0)
				{
					LSLogger.Log("DebugSampleSystem", GlobalValues.DebugSampleSystem.ToString());
				}
				else
				{
					string arg = x[0].ToLower();
					if (arg == "true")
						GlobalValues.DebugSampleSystem = true;
					else if (arg == "extended")
					{
						GlobalValues.DebugSampleSystem = true;
						GlobalValues.DebugSampleSystemExtended = true;
					}
					else if (arg == "false")
					{
						GlobalValues.DebugSampleSystem = false;
						GlobalValues.DebugSampleSystemExtended = false;
					}
					else
						LSLogger.Log("DebugSampleSystem", "Invalid argument - usage: debugsamplesystem [true:extended:false]");
				}
			}));

			consoleCommands.Add(new Console.ConsoleCommand("debugplaybackcontroller", (x) =>
			{
				if (x.Count > 1)
				{
					LSLogger.Log("DebugPlaybackController", "Invalid argument count - usage: debugplaybackcontroller [true:false]");
				}
				else if (x.Count == 0)
				{
					LSLogger.Log("DebugPlaybackController", GlobalValues.DebugPlaybackController.ToString());
				}
				else
				{
					if (x[0].ToLower() == "true")
						GlobalValues.DebugPlaybackController = true;
					else if (x[0].ToLower() == "false")
						GlobalValues.DebugPlaybackController = false;
					else
						LSLogger.Log("DebugPlaybackController", "Invalid argument - usage: debugplaybackcontroller [true:false]");
				}
			}));

			consoleCommands.Add(new Console.ConsoleCommand("debugtimelinereader", (x) =>
			{
				if (x.Count > 1)
				{
					LSLogger.Log("DebugTimelineReader", "Invalid argument count - usage: debugtimelinereader [true:extended:false]");
				}
				else if (x.Count == 0)
				{
					LSLogger.Log("DebugTimelineReader", GlobalValues.DebugTimelineReader.ToString());
				}
				else
				{
					string arg = x[0].ToLower();
					if (arg == "true")
						GlobalValues.DebugTimelineReader = true;
					else if (arg == "extended")
					{
						GlobalValues.DebugTimelineReader = true;
						GlobalValues.DebugTimelineReaderExtended = true;
					}
					else if (arg == "false")
					{
						GlobalValues.DebugTimelineReader = false;
						GlobalValues.DebugTimelineReaderExtended = false;
					}
					else
						LSLogger.Log("DebugTimelineReader", "Invalid argument - usage: debugtimelinereader [true:extended:false]");
				}
			}));

			consoleCommands.Add(new Console.ConsoleCommand("debugplaybacksystem", (x) =>
			{
				if (x.Count > 1)
				{
					LSLogger.Log("DebugPlaybackSystem", "Invalid argument count - usage: debugplaybacksystem [true:false]");
				}
				else if (x.Count == 0)
				{
					LSLogger.Log("DebugPlaybackSystem", GlobalValues.DebugPlaybackSystem.ToString());
				}
				else
				{
					if (x[0].ToLower() == "true")
						GlobalValues.DebugPlaybackSystem = true;
					else if (x[0].ToLower() == "false")
						GlobalValues.DebugPlaybackSystem = false;
					else
						LSLogger.Log("DebugPlaybackSystem", "Invalid argument - usage: debugplaybacksystem [true:false]");
				}
			}));

			consoleCommands.Add(new Console.ConsoleCommand("mirrortologfile", (x) =>
			{
				if (x.Count > 1)
				{
					LSLogger.Log("MirrorToLogFile", "Invalid argument count - usage: mirrortologfile [true:false]");
				}
				else if (x.Count == 0)
				{
					LSLogger.Log("DebugPlaybaMirrorToLogFileckSystem", GlobalValues.MirrorConsoleToLogFile.ToString());
				}
				else
				{
					if (x[0].ToLower() == "true")
						GlobalValues.MirrorConsoleToLogFile = true;
					else if (x[0].ToLower() == "false")
					{
						GlobalValues.MirrorConsoleToLogFile = false;
						ControllerMaster.instance.GetCurrentController<ConsoleController>("console").CloseLogFileHandle();
					}
					else
						LSLogger.Log("MirrorToLogFile", "Invalid argument - usage: mirrortologfile [true:false]");
				}
			}));

			consoleCommands.Add(new Console.ConsoleCommand("screenshot", (x) =>
			{
				if (x.Count != 1)
				{
					LSLogger.Log("Screenshot", "Invalid argument count - usage: screenshot [scale factor]");
					return;
				}

				int factor = Convert.ToInt32(x[0]);

				ControllerMaster.instance.DisableUI("console");

				int counter = 0;
				UpdateSystem.instance.AddUpdateAction("screenshot_delayed_action", () =>
				{
					counter++;
					if (counter > 1)
					{
						string path = @"Screenshot-" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
						ScreenCapture.CaptureScreenshot(path, factor);
						UpdateSystem.instance.RemoveUpdateAction("screenshot_delayed_action");

						UpdateSystem.instance.AddUpdateAction("screenshot_delayed_action2", () =>
						{
							ControllerMaster.instance.EnableUI("console");
							UpdateSystem.instance.RemoveUpdateAction("screenshot_delayed_action2");
							LSLogger.Log("Screenshot", "Saved screenshot to file '" + path + "'.");
						});
					}
				});
			}));

			return consoleCommands;
		}
	}
}
