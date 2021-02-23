
namespace VisualizingTheDatabike.DataModel
{
    /// <summary>
    /// Class representing a settings configuration.
    /// </summary>
    public class Settings
    {
        public string Resolution { get; set; }
        public int FullscreenMode { get; set; } //like UnityEngine.FullScreenMode enum but 2 = 3
        public string FPSLimit { get; set; }
        public float UIScale { get; set; }
        public float PedalOffset { get; set; }
        public bool ShowStatusBars { get; set; }
        public bool ShowGearsIndicator { get; set; }
        public bool ShowSteeringIndicator { get; set; }
        public bool ShowArtificialHorizon { get; set; }
        public bool ShowCompass { get; set; }
    }
}
