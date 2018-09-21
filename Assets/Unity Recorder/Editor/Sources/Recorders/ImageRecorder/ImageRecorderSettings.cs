using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{   
    public enum ImageRecorderOutputFormat
    {
        PNG,
        JPEG,
        EXR
    }
    
    [RecorderSettings(typeof(ImageRecorder), "Image Sequence", "imagesequence_16")]
    public class ImageRecorderSettings : RecorderSettings
    {
        public ImageRecorderOutputFormat outputFormat = ImageRecorderOutputFormat.JPEG;
        public bool captureAlpha;

        [SerializeField] ImageInputSelector m_ImageInputSelector = new ImageInputSelector();

        public ImageRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get
            {
                switch (outputFormat)
                {
                    case ImageRecorderOutputFormat.PNG:                        
                        return "png";
                    case ImageRecorderOutputFormat.JPEG:                        
                        return "jpg";
                    case ImageRecorderOutputFormat.EXR:                        
                        return "exr";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public ImageInputSettings imageInputSettings
        {
            get { return m_ImageInputSelector.imageInputSettings; }
            set { m_ImageInputSelector.imageInputSettings = value; }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
 
            if(string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override IEnumerable<RecorderInputSettings> inputsSettings
        {
            get { yield return m_ImageInputSelector.selected; }
        }

        public override void SelfAdjustSettings()
        {
            var input = m_ImageInputSelector.selected;
            
            if (input == null)
                return;

            var renderTextureSamplerSettings = input as RenderTextureSamplerSettings;
            if (renderTextureSamplerSettings != null)
            {
                var colorSpace = outputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                renderTextureSamplerSettings.colorSpace = colorSpace;
            }
            
            var cbis = input as CameraInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = (outputFormat == ImageRecorderOutputFormat.PNG || outputFormat == ImageRecorderOutputFormat.EXR) && captureAlpha;
            }
        }
    }
}
