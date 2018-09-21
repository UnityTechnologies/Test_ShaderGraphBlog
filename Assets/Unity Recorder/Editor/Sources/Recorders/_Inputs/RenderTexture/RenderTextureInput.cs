using System;

namespace UnityEditor.Recorder.Input
{
    class RenderTextureInput : BaseRenderTextureInput
    {
        TextureFlipper m_VFlipper;
        
        RenderTextureInputSettings cbSettings
        {
            get { return (RenderTextureInputSettings)settings; }
        }

        public override void BeginRecording(RecordingSession session)
        {
            if (cbSettings.renderTexture == null)
                throw new Exception("No Render Texture object provided as source");
            
            outputHeight = cbSettings.outputHeight;
            outputWidth = cbSettings.outputWidth;
            
            outputRT = cbSettings.renderTexture;
            
            if (cbSettings.flipFinalOutput)
                m_VFlipper = new TextureFlipper();
        }

        public override void NewFrameReady(RecordingSession session)
        {
            if (cbSettings.flipFinalOutput)
                m_VFlipper.Flip(outputRT);
            
            base.NewFrameReady(session);
        }
        
    }
}