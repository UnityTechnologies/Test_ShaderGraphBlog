using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Targeted Camera")]
    [Serializable]
    public class CameraInputSettings : StandardImageInputSettings
    {
        public ImageSource source = ImageSource.ActiveCamera;
        public string cameraTag;
        public bool flipFinalOutput;
        public bool captureUI;

        public CameraInputSettings()
        {
            outputImageHeight = ImageHeight.Window;
        }
        
        internal override Type inputType
        {
            get { return typeof(CameraInput); }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            if (source == ImageSource.TaggedCamera && string.IsNullOrEmpty(cameraTag))
            {
                ok = false;
                errors.Add("Missing tag for camera selection");
            }

            return ok;
        }
    }
}
