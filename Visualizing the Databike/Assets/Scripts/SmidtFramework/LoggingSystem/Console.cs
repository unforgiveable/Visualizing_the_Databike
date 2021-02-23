using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.Services;

namespace SmidtFramework.LoggingSystem
{
	/// <summary>
	/// Class for managing registration and execution of console commands and output to a UI Text field.
	/// </summary>
	public class Console
	{
		#region types
		public delegate void CommandAction(List<string> args);
		public struct ConsoleCommand
		{
			public string identifier;
			public CommandAction action;

			public ConsoleCommand(string identifier, CommandAction action)
			{
				this.identifier = identifier;
				this.action = action;
			}
		}
		#endregion

		#region fields
		private const string LogFilePath = @"log.txt";

		private Text mainText;
		private int maxLineCount;
		private int numSavedInputs;
		private string prefix;

		private int lineCount;
		private Dictionary<string, CommandAction> commands;
		private List<string> lastInputs;

		private StreamWriter logFileHandle;
		#endregion

		/// <summary>
		/// Creates a new Console based on the provided Text component with the specified properties.
		/// </summary>
		/// <param name="_text">Text component reference.</param>
		/// <param name="_fontSize">Font size.</param>
		/// <param name="_numSavedInputs">Number of last inputs to be saved.</param>
		/// <param name="_prefix">Prefix to be written infront of each line.</param>
		public Console(Text _text, int _fontSize = 12, int _numSavedInputs = 5, string _prefix = ">")
		{
			if (_text == null || _fontSize <= 0 || _numSavedInputs <= 0 || string.IsNullOrEmpty(_prefix))
			{
				throw new ArgumentException("Illegal Arguments for Console creation.");
			}

			mainText = _text;
			mainText.text = "";
			mainText.fontSize = _fontSize;
			mainText.alignment = TextAnchor.LowerLeft;
			//MainText.color = Color.white;
			mainText.horizontalOverflow = HorizontalWrapMode.Overflow;

			numSavedInputs = _numSavedInputs;
			prefix = _prefix;
			commands = new Dictionary<string, CommandAction>();
			lastInputs = new List<string>();

			CalculateMaxLineCount(); //calculate maximum number of lines
			//Write("MaxLineCount: " +maxLineCount);
		}

		#region public_functions
		/// <summary>
		/// Writes the string s to the console in a new line.
		/// </summary>
		/// <param name="s">String to be written.</param>
		public void Write(string s)
		{
			if (string.IsNullOrEmpty(s)) return;

			if (s.Contains("\n")) //find new line chars and increase linecount accordingly
				lineCount += s.Split('\n').Length - 1;

			string text = prefix + s + "\n";
			mainText.text += text; //add prefix and new line, add to text
			lineCount++;
			LimitLinesToMax(); //remove lines over maximum from the front

			if (GlobalValues.MirrorConsoleToLogFile)
			{
				if (logFileHandle == null)
				{
					logFileHandle = new StreamWriter(LogFilePath);
				}

				logFileHandle.Write(text);
				logFileHandle.Flush();
			}
		}

		/// <summary>
		/// Adds the provided ConsoleCommand to the Console. The command identifier is case-insensitive. Throws an ArgumentException if any values are invalid.
		/// </summary>
		/// <param name="com">Command to be added.</param>
		public void AddCommand(ConsoleCommand com)
		{
			AddCommand(com.identifier, com.action);
		}

		/// <summary>
		/// Adds a new command with identifier and action to the console. The command identifier is case-insensitive. Throws an ArgumentException if any values are invalid.
		/// </summary>
		/// <param name="identifier">Identifer of the command.</param>
		/// <param name="action">Action of the command.</param>
		public void AddCommand(string identifier, CommandAction action)
		{
			if (string.IsNullOrEmpty(identifier) || action == null)
			{
				throw new ArgumentException("Invalid values for console command to be added.");
			}

			identifier = identifier.ToLower();

			if (commands.ContainsKey(identifier))
			{
				throw new ArgumentException("Command with identifier '" + identifier + "' already exists.");
			}

			commands.Add(identifier, action);
		}

		/// <summary>
		/// Executes the ConsoleCommand with input as identifier (not counting the arguments). Arguments are passed as list to the Command.
		/// If no matching Command is found outputs error to Console. For more information please see the Documentation.
		/// </summary>
		/// <param name="input">Input.</param>
		public void ExecuteCommand(string input)
		{
			Write(input); //write input to console

			lastInputs.Insert(0, input); //save to last inputs
			if (lastInputs.Count > numSavedInputs)
				lastInputs.RemoveAt(numSavedInputs);

			input = input.ToLower(); //normalize input

			List<string> args = input.Split(' ').ToList(); //seperate arguments

			string identifier = args[0];
			if (!commands.ContainsKey(identifier))
			{
				Write("<color=red>ERROR</color> - unknown command '" + identifier + "'");
				return;
			}

			args.RemoveAt(0); //remove command from list of arguments
			commands[identifier].Invoke(args); //call function
		}

		/// <summary>
		/// Returns a list of the last processed inputs. Used for recalling inputs. Number of inputs saved can be set on Console creation.
		/// </summary>
		/// <returns>The last inputs.</returns>
		public List<string> GetLastInputs()
		{
			return lastInputs;
		}

		/// <summary>
		/// Returns a list of command identifiers containing input, ordered by position of occurrence within the identifier. Used for autocompletion of commands.
		/// </summary>
		/// <param name="input">Partial input to be matched against registered commands.</param>
		/// <returns>The matching command identifiers.</returns>
		public List<string> GetMatchingCommands(string input)
		{
			//get all command identifiers containing the input and order by position of the occurrence.
			return commands.Keys.Where(x => x.Contains(input)).OrderBy(x => x.IndexOf(input)).ToList();
		}

		/// <summary>
		/// Closes the logfile handle if necessary.f
		/// </summary>
		public void CloseLogFileHandle()
		{
			logFileHandle?.Close();
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Checks if LineCount > MaxLineCount, if so removes LC-MLC lines from front of content.
		/// </summary>
		private void LimitLinesToMax()
		{
			if (lineCount > maxLineCount)
			{
				int count = 0;
				int nl = 0;
				string s = mainText.text;

				while (nl < lineCount - maxLineCount) //count chars to last \n to be removed
				{
					count++;
					if (s[count] == '\n')
						nl++;
				}
				mainText.text = s.Remove(0, count + 1);
				lineCount = maxLineCount;
			}
		}

		/// <summary>
		/// Calculates the maximum number of lines that fit within the text UI.
		/// </summary>
		private void CalculateMaxLineCount()
		{
			TextGenerator textGenerator = new TextGenerator();
			var generationSettings = mainText.GetGenerationSettings(mainText.rectTransform.rect.size);
			int lineCount = 0;
			StringBuilder s = new StringBuilder();
			while (true)
			{
				s.Append("\n");
				textGenerator.Populate(s.ToString(), generationSettings);
				int nextLineCount = textGenerator.lineCount;
				if (lineCount == nextLineCount)
					break;
				lineCount = nextLineCount;
			}
			maxLineCount = lineCount;
		}
		#endregion
	}
}
