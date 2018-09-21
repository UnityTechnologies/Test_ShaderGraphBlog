using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    class GameViewInput : RecorderInput
    {
        bool m_ModifiedResolution;

        public Texture2D image { get; private set; }

        GameViewInputSettings scSettings
        {
            get { return (GameViewInputSettings)settings; }
        }

        public int outputWidth { get; private set; }
        public int outputHeight { get; private set; }

        public override void NewFrameReady(RecordingSession session)
        {
            image = ScreenCapture.CaptureScreenshotAsTexture();
        }

        public override void BeginRecording(RecordingSession session)
        {
            outputWidth = scSettings.outputWidth;
            outputHeight = scSettings.outputHeight;
            
            int w, h;
            GameViewSize.GetGameRenderSize(out w, out h);
            if (w != outputWidth || h != outputHeight)
            {
                var size = GameViewSize.SetCustomSize(outputWidth, outputHeight) ?? GameViewSize.AddSize(outputWidth, outputHeight);
                if (GameViewSize.modifiedResolutionCount == 0)
                    GameViewSize.BackupCurrentSize();
                else
                {
                    if (size != GameViewSize.currentSize)
                    {
                        Debug.LogError("Requestion a resultion change while a recorder's input has already requested one! Undefined behaviour.");
                    }
                }
                GameViewSize.modifiedResolutionCount++;
                m_ModifiedResolution = true;
                GameViewSize.SelectSize(size);
            }
        }

        public override void FrameDone(RecordingSession session)
        {
            UnityHelpers.Destroy(image);
            image = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_ModifiedResolution)
                {
                    GameViewSize.modifiedResolutionCount--;
                    if (GameViewSize.modifiedResolutionCount == 0)
                        GameViewSize.RestoreSize();
                }
            }

            base.Dispose(disposing);
        }
    }
}