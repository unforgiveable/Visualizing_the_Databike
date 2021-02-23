using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.LoggingSystem;
using SmidtFramework.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Class for managing the load timeline menu. ID "timeline_menu".
	/// </summary>
	public class TimelineMenuController : BaseUIController
	{
		private enum SortOption
		{
			nameDec,
			nameAsc,
			dateDec,
			dateAsc
		}

		private class Entry
		{
			public string name;
			public string date;
			public string path;
			public GameObject entryObject;
		}

		#region fields
		//constants
		private const string timelineFolder = @"Timelines/";
		private const float entrySpacing = 5f;
		private const float entryHeight = 40f;
		private readonly Color32 entryBaseColor = new Color32(200, 200, 200, 255);
		private readonly Color32 entryHighlightColor = new Color32(230, 230, 230, 255);

		//cached references
		private Transform sortArrowName;
		private Transform sortArrowDate;
		private Scrollbar scrollbar;
		private RectTransform contentPanel;
		private GameObject entryPrefab;

		private SortOption selectedSortOption = SortOption.nameDec;
		private List<Entry> currentEntries = new List<Entry>();
		private Entry currentSelectedEntry = null;
		#endregion

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/TimelineMenu/TimelineMenu";
		}

		public override void Init()
		{
			sortArrowName = WorldObject.transform.GetChild(1).GetChild(0).GetChild(1);
			sortArrowDate = WorldObject.transform.GetChild(1).GetChild(1).GetChild(1);
			scrollbar = WorldObject.transform.GetChild(3).GetComponent<Scrollbar>();
			contentPanel = WorldObject.transform.GetChild(2).GetChild(0).GetComponent<RectTransform>();
			entryPrefab = Resources.Load<GameObject>("Prefabs/UI/TimelineMenu/TimelineEntry");

			//Name sort button
			WorldObject.transform.GetChild(1).GetChild(0).GetComponent<ButtonScript>().ClickAction = () =>
			{
				if (selectedSortOption == SortOption.nameDec)
				{
					SelectSortOption(SortOption.nameAsc);
				}
				else
				{
					SelectSortOption(SortOption.nameDec);
				}
			};
			//Date sort button
			WorldObject.transform.GetChild(1).GetChild(1).GetComponent<ButtonScript>().ClickAction = () =>
			{
				if (selectedSortOption == SortOption.dateDec)
				{
					SelectSortOption(SortOption.dateAsc);
				}
				else
				{
					SelectSortOption(SortOption.dateDec);
				}
			};

			//back button
			WorldObject.transform.GetChild(4).GetComponent<ButtonScript>().ClickAction = () => BackButtonAction();

			//refresh button
			WorldObject.transform.GetChild(5).GetComponent<ButtonScript>().ClickAction = () =>
			{
				LoadTimelineEntries();
			};
			//load button
			WorldObject.transform.GetChild(6).GetComponent<ButtonScript>().ClickAction = () =>
			{
				if (currentSelectedEntry == null)
				{
					LSLogger.Log("TimelineMenu", "Load with no entry selected - ignoring.");
					return;
				}

				LSLogger.Log("TimelineMenu", "Loading timeline '" + currentSelectedEntry.name + "'.");
				try
				{
					SceneController sc = ControllerMaster.instance.GetCurrentController<SceneController>("scene");
					sc.LoadTimeline(currentSelectedEntry.path);
					sc.SwitchConfigToMainWorld();
				}
				catch (Exception e)
				{
					LSLogger.LogError("TimelineMenu error loading timeline - " + e.ToString() + " - " + e.Message);
					UniversalDialogueController universalDialogueController = ControllerMaster.instance.CreateController<UniversalDialogueController>("universal_dialogue");
					universalDialogueController.SetupWindow("An error occured while loading the timeline.\nPlease check the console (F9) for details.", "Okay", null);
				}
			};
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
			InputSystem.instance.RemoveKeyAction(KeyCode.Escape, "timeline_menu_escape");
			InputSystem.instance.RemoveInputLock("timeline_menu_lock");
		}

		public override void OnEnable()
		{
			LoadTimelineEntries();

			InputSystem.instance.AddKeyAction("timeline_menu_escape", KeyCode.Escape, InputSystem.KeyPressType.onDown, () => BackButtonAction());

			InputSystem.instance.AddInputLock("timeline_menu_lock", new string[] { "timeline_menu_escape", "consoleToggle" });
		}
		#endregion

		#region private_functions
		/// <summary>
		/// Loads all gpx files from the timelineFolder and creates list entries for each one. Sorts the list accoring to the selectedSortType.
		/// </summary>
		private void LoadTimelineEntries()
		{
			if (currentEntries.Count != 0)
				ClearTimelineEntries();

			//get all gpx files in folder
			string[] filePaths;
			try
			{
				if (!Directory.Exists(timelineFolder))
				{
					Directory.CreateDirectory(timelineFolder);
				}

				filePaths = Directory.GetFiles(timelineFolder, "*.gpx", SearchOption.TopDirectoryOnly);
			}
			catch (Exception e)
			{
				LSLogger.LogError("TimelineMenu error while loading timelines - " + e.ToString() + " - " + e.Message);
				return;
			}

			//create entries for each
			foreach (string path in filePaths)
			{
				Entry entry = new Entry
				{
					name = Path.GetFileName(path),
					date = File.GetCreationTime(path).ToString("u"),
					path = path
				};

				GameObject newObject = UnityEngine.Object.Instantiate(entryPrefab);
				newObject.transform.SetParent(contentPanel, false);
				newObject.transform.GetChild(0).GetComponent<Text>().text = entry.name;
				newObject.transform.GetChild(1).GetComponent<Text>().text = entry.date;
				entry.entryObject = newObject;

				newObject.GetComponent<ButtonScript>().ClickAction = () =>
				{
					SelectEntry(entry);
				};

				currentEntries.Add(entry);
			}
			contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, entrySpacing + (currentEntries.Count * (entryHeight + entrySpacing)));
			scrollbar.value = 1;

			SortEntries();
		}

		/// <summary>
		/// Removes all currently shown entries from the list.
		/// </summary>
		private void ClearTimelineEntries()
		{
			foreach (Entry e in currentEntries)
			{
				UnityEngine.Object.Destroy(e.entryObject);
			}
			currentEntries.Clear();
			contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, 0);
			currentSelectedEntry = null;
		}

		/// <summary>
		/// Clears the previously selected entry and slects the provided one instead.
		/// </summary>
		/// <param name="entry">Entry to select.</param>
		private void SelectEntry(Entry entry)
		{
			ClearSelectedEntry();

			entry.entryObject.GetComponent<Image>().color = entryHighlightColor;
			currentSelectedEntry = entry;
		}

		/// <summary>
		/// Removes the highlighting from the currently selected entry and deselects it.
		/// </summary>
		private void ClearSelectedEntry()
		{
			if (currentSelectedEntry != null)
			{
				currentSelectedEntry.entryObject.GetComponent<Image>().color = entryBaseColor;
				currentSelectedEntry = null;
			}
		}

		/// <summary>
		/// Sorts the currentEntries list according to the selectedSortOption and reorders the list entries to match.
		/// </summary>
		private void SortEntries()
		{
			//sort entry list
			switch (selectedSortOption)
			{
				case SortOption.nameDec:
					currentEntries.Sort((x, y) => string.Compare(x.name, y.name));
					break;
				case SortOption.nameAsc:
					currentEntries.Sort((x, y) => string.Compare(y.name, x.name));
					break;
				case SortOption.dateDec:
					currentEntries.Sort((x, y) => string.Compare(x.date, y.date));
					break;
				case SortOption.dateAsc:
					currentEntries.Sort((x, y) => string.Compare(y.date, x.date));
					break;
			}

			//set list entry object position
			for (int i = 0; i < currentEntries.Count; i++)
			{
				RectTransform rt = currentEntries[i].entryObject.GetComponent<RectTransform>();
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -i * (entryHeight + entrySpacing) - entrySpacing);
			}

			ClearSelectedEntry();
		}

		/// <summary>
		/// Changes the sort option to the new option, updating the sort button arrows, and sorting the current entries.
		/// </summary>
		/// <param name="sortOption"></param>
		private void SelectSortOption(SortOption sortOption)
		{
			//hide / show / rotate sorting arrows to fit
			switch (sortOption)
			{
				case SortOption.nameDec:
					sortArrowName.gameObject.SetActive(true);
					sortArrowName.rotation = Quaternion.Euler(0, 0, 0);
					sortArrowDate.gameObject.SetActive(false);
					break;
				case SortOption.nameAsc:
					sortArrowName.gameObject.SetActive(true);
					sortArrowName.rotation = Quaternion.Euler(0, 0, 180);
					sortArrowDate.gameObject.SetActive(false);
					break;
				case SortOption.dateDec:
					sortArrowName.gameObject.SetActive(false);
					sortArrowDate.gameObject.SetActive(true);
					sortArrowDate.rotation = Quaternion.Euler(0, 0, 0);
					break;
				case SortOption.dateAsc:
					sortArrowName.gameObject.SetActive(false);
					sortArrowDate.gameObject.SetActive(true);
					sortArrowDate.rotation = Quaternion.Euler(0, 0, 180);
					break;
			}

			//sort entries
			selectedSortOption = sortOption;
			SortEntries();
		}

		private void BackButtonAction()
		{
			ControllerMaster.instance.DisableUI("timeline_menu");
			ControllerMaster.instance.EnableUI("main_menu");
		}
		#endregion
	}
}
