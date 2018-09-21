using System;

namespace UnityEditor.Recorder.FrameCapturer
{
    class EXRRecorder : GenericRecorder<EXRRecorderSettings>
    {
        static readonly string[] s_channelNames = { "R", "G", "B", "A" };
        fcAPI.fcExrContext m_ctx;

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            m_Settings.fileNameGenerator.CreateDirectory(session);

            m_ctx = fcAPI.fcExrCreateContext(ref m_Settings.m_ExrEncoderSettings);
            return m_ctx;
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
            
            var path = m_Settings.fileNameGenerator.BuildAbsolutePath(session);

            var input = (BaseRenderTextureInput)m_Inputs[0];
            var frame = input.outputRT;
            fcAPI.fcLock(frame, (data, fmt) =>
            {
                fcAPI.fcExrBeginImage(m_ctx, path, frame.width, frame.height);
                int channels = (int)fmt & 7;
                for (int i = 0; i < channels; ++i)
                {
                    fcAPI.fcExrAddLayerPixels(m_ctx, data, fmt, i, s_channelNames[i]);
                }
                fcAPI.fcExrEndImage(m_ctx);
            });
        }

    }
}
