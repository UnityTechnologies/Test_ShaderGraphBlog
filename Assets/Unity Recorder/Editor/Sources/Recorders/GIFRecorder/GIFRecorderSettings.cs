using UnityEditor.Recorder.FrameCapturer;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [RecorderSettings(typeof(GIFRecorder), "GIF Animation", "imagesequence_16")]
    public class GIFRecorderSettings : BaseFCRecorderSettings
    {
        [SerializeField] internal fcAPI.fcGifConfig gifEncoderSettings = fcAPI.fcGifConfig.default_value;

        public int numColors
        {
            get { return gifEncoderSettings.numColors; }
            set { gifEncoderSettings.numColors = Mathf.Clamp(value, 1, 256); }
        }
        
        public int keyframeInterval
        {
            get { return gifEncoderSettings.keyframeInterval; }
            set { gifEncoderSettings.keyframeInterval = Mathf.Clamp(value, 1, 120); }
        }
        
        public int maxTasks
        {
            get { return gifEncoderSettings.maxTasks; }
            set { gifEncoderSettings.maxTasks = Mathf.Clamp(value, 1, 32); }
        }

        public GIFRecorderSettings()
        {
            fileNameGenerator.fileName = "gif_animation_" + DefaultWildcard.Take;
            
            m_ImageInputSelector.cameraInputSettings.flipFinalOutput = true;
            m_ImageInputSelector.renderTextureInputSettings.flipFinalOutput = true;
            m_ImageInputSelector.renderTextureSamplerSettings.flipFinalOutput = true;
        }
        
        public override string extension
        {
            get { return "gif"; }
        }
    }
}
