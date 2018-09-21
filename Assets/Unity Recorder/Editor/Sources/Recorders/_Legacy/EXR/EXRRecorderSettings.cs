namespace UnityEditor.Recorder.FrameCapturer
{
    [RecorderSettings(typeof(EXRRecorder), "Legacy/OpenEXR")]
    class EXRRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcExrConfig m_ExrEncoderSettings = fcAPI.fcExrConfig.default_value;

        public EXRRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get { return "exr"; }
        }
    }
}
