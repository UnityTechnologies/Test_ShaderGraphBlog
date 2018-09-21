namespace UnityEditor.Recorder.FrameCapturer
{
    [RecorderSettings(typeof(PNGRecorder), "Legacy/PNG" )]
    class PNGRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcPngConfig m_PngEncoderSettings = fcAPI.fcPngConfig.default_value;

        public PNGRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get { return "png"; }
        }
    }
}
