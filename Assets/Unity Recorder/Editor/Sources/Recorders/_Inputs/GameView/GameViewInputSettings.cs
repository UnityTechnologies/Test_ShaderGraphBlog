using System;
using System.ComponentModel;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Game View")]
    [Serializable]
    public class GameViewInputSettings : StandardImageInputSettings
    {
        public GameViewInputSettings()
        {
            outputImageHeight = ImageHeight.Window;
        }
        
        internal override Type inputType
        {
            get { return typeof(GameViewInput); }
        }

        public override bool supportsTransparent
        {
            get { return false; }
        }
    }
}