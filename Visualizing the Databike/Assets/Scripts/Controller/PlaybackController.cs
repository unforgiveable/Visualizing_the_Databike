using SmidtFramework.ControllerSystem;
using SmidtFramework.InputSystem;
using SmidtFramework.LoggingSystem;
using SmidtFramework.Utility;
using UnityEngine;
using UnityEngine.UI;
using VisualizingTheDatabike.DataModel;
using VisualizingTheDatabike.Services;
using VisualizingTheDatabike.Utility;

namespace VisualizingTheDatabike.Controllers
{
	/// <summary>
	/// Controller for the playback control UI.
	/// </summary>
	public class PlaybackController : BaseUIController, IBikePlaybackVisualizer
	{
		#region fields
		private PlaybackSystem playbackSystem;

		private Text timeText;
		private Image playButton;
		private Slider timelineSlider;
		private Text speedText;

		private Sprite s_play;
		private Sprite s_pause;

		private int speedButtonState = 0;
		private float sliderValue = 0;
		private bool isDragging = false;
		#endregion

		#region controller_overrides
		public override string GetPrefabPath()
		{
			return "Prefabs/UI/PlaybackUI";
		}

		public override void Init()
		{
			playbackSystem = ControllerMaster.instance.GetCurrentController<SceneController>("scene").PlaybackSystem;
			
			//pre-load images for play button
			s_play = Resources.Load<Sprite>("UI/PlayButton_play");
			s_pause = Resources.Load<Sprite>("UI/PlayButton_pause");

			//find references
			timeText = WorldObject.transform.GetChild(0).GetComponent<Text>();
			playButton = WorldObject.transform.GetChild(1).GetComponent<Image>();
			timelineSlider = WorldObject.transform.GetChild(2).GetComponent<Slider>();
			speedText = WorldObject.transform.GetChild(3).GetChild(0).GetComponent<Text>();

			//set default values
			timeText.text = "00:00:00.000";
			playButton.sprite = s_play;
			timelineSlider.value = 0;
			speedText.text = "1x";

			//play button function
			playButton.GetComponent<ButtonScript>().ClickAction = () =>
			{
				if (GlobalValues.DebugPlaybackController) LSLogger.Log("PlaybackController", "Play button pressed.");
				playbackSystem.SwitchPlayback();
			};

			//timeline slider interaction function
			EventScript sliderEvents = timelineSlider.GetComponent<EventScript>();
			sliderEvents.SetEvent(0, () => //OnBeginDrag
			{
				if (GlobalValues.DebugPlaybackController) LSLogger.Log("PlaybackController", "Slider begin drag event. "+timelineSlider.value);
				playbackSystem.PausePlayback();
				isDragging = true;
			});
			sliderEvents.SetEvent(1, () => //OnEndDrag
			{
				if (GlobalValues.DebugPlaybackController) LSLogger.Log("PlaybackController", "Slider end drag event. "+timelineSlider.value);
				playbackSystem.SetCurrentTime(timelineSlider.value * playbackSystem.TimelineLength);
				isDragging = false;
			});
			sliderEvents.SetEvent(2, () => //OnClick
			{
				if (GlobalValues.DebugPlaybackController) LSLogger.Log("PlaybackController", "Slider click event.");
				if (isDragging) return;

				playbackSystem.PausePlayback();
				playbackSystem.SetCurrentTime(sliderValue * playbackSystem.TimelineLength);
			});
			sliderEvents.SetEvent(3, () => //OnPointerDown - save slider position for click
			{
				if (GlobalValues.DebugPlaybackController) LSLogger.Log("PlaybackController", "Slider pointer down. "+timelineSlider.value);
				sliderValue = timelineSlider.value;
			});


			//speed button function
			WorldObject.transform.GetChild(3).GetComponent<ButtonScript>().ClickAction = () =>
			{
				speedButtonState = (speedButtonState + 1) % 5;

				float mult;
				switch(speedButtonState)
				{
					case 0:
						mult = 1;
						speedText.text = "1x";
						break;
					case 1:
						mult = 0.5f;
						speedText.text = "1/2x";
						break;
					case 2:
						mult = 0.25f;
						speedText.text = "1/4x";
						break;
					case 3:
						mult = 1f/8f;
						speedText.text = "1/8x";
						break;
					case 4:
						mult = 1f/16f;
						speedText.text = "1/16x";
						break;
					default:
						LSLogger.LogError("PlaybackController speed button change invalid state '" + speedButtonState + "'.");
						mult = 1f;
						speedText.text = "ERR";
						break;
				}
				playbackSystem.SetCurrentSpeed(mult);
			};

			//version text
			WorldObject.transform.GetChild(4).GetComponent<Text>().text = "Version " + Application.version;
		}

		public override void OnDelete()
		{
		}

		public override void OnDisable()
		{
			InputSystem.instance.RemoveKeyAction(KeyCode.Space, "playback_pause_toggle");
		}

		public override void OnEnable()
		{
			InputSystem.instance.AddKeyAction("playback_pause_toggle", KeyCode.Space, InputSystem.KeyPressType.onDown, () =>
			{
				playbackSystem.SwitchPlayback();
			});
		}
		#endregion

		#region public_functions
		public void UpdateWithNewBikeState(BikeState newState)
		{
			//update current time text
			timeText.text = FormatUtility.GetTimeStringFromSeconds(newState.Time);

			//update timeline slider
			timelineSlider.value = newState.Time / playbackSystem.TimelineLength;
		}

		public void UpdatePlaybackState(bool newState)
		{
			UpdatePlayButton(newState);
		}
		#endregion

		/// <summary>
		/// Updates the play button to match the current state.
		/// </summary>
		private void UpdatePlayButton(bool newState)
		{
			if (newState)
				playButton.sprite = s_pause;
			else
				playButton.sprite = s_play;
		}
	}
}