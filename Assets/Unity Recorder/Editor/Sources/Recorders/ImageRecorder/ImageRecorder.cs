using System;
using System.IO;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class ImageRecorder : GenericRecorder<ImageRecorderSettings>
    {

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            m_Settings.fileNameGenerator.CreateDirectory(session);

            return true;
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            Texture2D tex = null;
            if (m_Inputs[0] is GameViewInput)
            {
                tex = ((GameViewInput)m_Inputs[0]).image;
                if (m_Settings.outputFormat == ImageRecorderOutputFormat.EXR)
                {
                    var textx = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat, false);
                    textx.SetPixels(tex.GetPixels());
                    tex = textx;
                }
                else if (m_Settings.outputFormat == ImageRecorderOutputFormat.PNG)
                {
                    var textx = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
                    textx.SetPixels(tex.GetPixels());
                    tex = textx;
                }
            }
            else
            {
                var input = (BaseRenderTextureInput)m_Inputs[0];
                var width = input.outputRT.width;
                var height = input.outputRT.height;
                tex = new Texture2D(width, height, m_Settings.outputFormat != ImageRecorderOutputFormat.EXR ? TextureFormat.RGBA32 : TextureFormat.RGBAFloat, false);
                var backupActive = RenderTexture.active;
                RenderTexture.active = input.outputRT;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                tex.Apply();
                RenderTexture.active = backupActive;
            }

            byte[] bytes;
            switch (m_Settings.outputFormat)
            {
                case ImageRecorderOutputFormat.PNG:
                    bytes = tex.EncodeToPNG();
                    break;
                case ImageRecorderOutputFormat.JPEG:
                    bytes = tex.EncodeToJPG();
                    break;
                case ImageRecorderOutputFormat.EXR:
                    bytes = tex.EncodeToEXR();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(m_Inputs[0] is BaseRenderTextureInput || m_Settings.outputFormat != ImageRecorderOutputFormat.JPEG)
                UnityHelpers.Destroy(tex);

            var path = m_Settings.fileNameGenerator.BuildAbsolutePath(session);

            File.WriteAllBytes( path, bytes);
        }
    }
}
