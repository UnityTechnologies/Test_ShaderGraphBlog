using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    public enum VideoRecorderOutputFormat
    {
        MP4,
        WEBM
    }

    [RecorderSettings(typeof(MovieRecorder), "Movie", "movie_16")]
    public class MovieRecorderSettings : RecorderSettings
    {
        public VideoRecorderOutputFormat outputFormat = VideoRecorderOutputFormat.MP4;
        public VideoBitrateMode videoBitRateMode = VideoBitrateMode.High;
        public bool captureAlpha;
        
        [SerializeField] ImageInputSelector m_ImageInputSelector = new ImageInputSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings = new AudioInputSettings();
        
        public MovieRecorderSettings()
        {
            fileNameGenerator.fileName = "movie";
            
            var iis = m_ImageInputSelector.selected as StandardImageInputSettings;
            if (iis != null)
                iis.maxSupportedSize = ImageHeight.x2160p_4K;
            
            m_ImageInputSelector.ForceEvenResolution(outputFormat == VideoRecorderOutputFormat.MP4);
        }

        public ImageInputSettings imageInputSettings
        {
            get { return m_ImageInputSelector.imageInputSettings; }
            set { m_ImageInputSelector.imageInputSettings = value; }
        }

        public AudioInputSettings audioInputSettings
        {
            get { return m_AudioInputSettings; }
        }

        public override IEnumerable<RecorderInputSettings> inputsSettings
        {
            get
            {
                yield return m_ImageInputSelector.selected;
                yield return m_AudioInputSettings;
            }
        }

        public override string extension
        {
            get { return outputFormat.ToString().ToLower(); }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);

            if (frameRatePlayback == FrameRatePlayback.Variable)
            {
                errors.Add("Movie recorder does not properly support Variable frame rate playback. Please consider using Constant frame rate instead");
                ok = false;
            }

            if (outputFormat == VideoRecorderOutputFormat.MP4)
            {
                var iis = m_ImageInputSelector.selected as ImageInputSettings;
                if (iis != null)
                {
                    if (iis.outputHeight % 2 != 0 || iis.outputWidth % 2 != 0)
                    {
                        errors.Add("Mp4 format does not support odd values in resolution");
                        ok = false;
                    }
                }
            }

            return ok;
        }

        public override void SelfAdjustSettings()
        {
            var selectedInput = m_ImageInputSelector.selected;
            if (selectedInput == null)
                return;

            var iis = selectedInput as StandardImageInputSettings;

            if (iis != null)
            {
                iis.maxSupportedSize = outputFormat == VideoRecorderOutputFormat.MP4
                    ? ImageHeight.x2160p_4K
                    : ImageHeight.x4320p_8K;

                if (iis.outputImageHeight != ImageHeight.Window && iis.outputImageHeight != ImageHeight.Custom)
                {
                    if (iis.outputImageHeight > iis.maxSupportedSize)
                        iis.outputImageHeight = iis.maxSupportedSize;
                }
            }

            var cbis = selectedInput as ImageInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = outputFormat == VideoRecorderOutputFormat.WEBM && captureAlpha;
            }
            
            m_ImageInputSelector.ForceEvenResolution(outputFormat == VideoRecorderOutputFormat.MP4);
        }
    }
}