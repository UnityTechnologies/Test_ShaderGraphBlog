using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder.Input;

namespace UnityEditor.Recorder.FrameCapturer
{
    /// <exclude/>
    public abstract class BaseFCRecorderSettings : RecorderSettings
    {
        [SerializeField] internal UTJImageInputSelector m_ImageInputSelector = new UTJImageInputSelector();

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if (string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }
        
        public ImageInputSettings imageInputSettings
        {
            get { return m_ImageInputSelector.imageInputSettings; }
            set { m_ImageInputSelector.imageInputSettings = value; }
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || 
                       Application.platform == RuntimePlatform.WindowsPlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer ||
                       Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.LinuxPlayer;
            }
        }

        public override IEnumerable<RecorderInputSettings> inputsSettings
        {
            get { yield return m_ImageInputSelector.selected; }
        }
    }
}
