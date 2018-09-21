namespace UnityEditor.Recorder.FrameCapturer
{
    [RecorderSettings(typeof(WEBMRecorder), "Legacy/WebM" )]
    class WEBMRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcWebMConfig m_WebmEncoderSettings = fcAPI.fcWebMConfig.default_value;
        public bool m_AutoSelectBR;

        public WEBMRecorderSettings()
        {
            fileNameGenerator.fileName = "movie";
            m_AutoSelectBR = true;
        }

        public override string extension
        {
            get { return "webm"; }
        }
    }
}
