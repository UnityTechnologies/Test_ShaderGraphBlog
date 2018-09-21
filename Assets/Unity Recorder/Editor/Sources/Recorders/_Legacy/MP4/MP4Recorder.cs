using System;
using UnityEngine;

namespace UnityEditor.Recorder.FrameCapturer
{
    class MP4Recorder : GenericRecorder<MP4RecorderSettings>
    {
        fcAPI.fcMP4Context m_ctx;
        
        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            m_Settings.fileNameGenerator.CreateDirectory(session);

            var input = (BaseRenderTextureInput)m_Inputs[0];
            if (input.outputWidth > 4096 || input.outputHeight > 2160 )
            {
                Debug.LogError("Mp4 format does not support requested resolution.");
            }

            return true;
        }

        public override void EndRecording(RecordingSession session)
        {
            m_ctx.Release();
            base.EndRecording(session);
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            var input = (BaseRenderTextureInput)m_Inputs[0];
            var frame = input.outputRT;

            if(!m_ctx)
            {
                var s = m_Settings.m_MP4EncoderSettings;
                s.video = true;
                s.audio = false;
                s.videoWidth = frame.width;
                s.videoHeight = frame.height;
                s.videoTargetFramerate = (int)Math.Ceiling(m_Settings.frameRate);
                if (m_Settings.m_AutoSelectBR)
                {
                    s.videoTargetBitrate = (int)(( (frame.width * frame.height/1000.0) / 245 + 1.16) * (s.videoTargetFramerate / 48.0 + 0.5) * 1000000);
                }
                var path = m_Settings.fileNameGenerator.BuildAbsolutePath(session);
                m_ctx = fcAPI.fcMP4OSCreateContext(ref s, path);
            }

            fcAPI.fcLock(frame, TextureFormat.RGB24, (data, fmt) =>
            {
                fcAPI.fcMP4AddVideoFramePixels(m_ctx, data, fmt, session.recorderTime);
            });
        }
    }
}
