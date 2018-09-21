using System;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class RecordingSession : IDisposable
    {   
        internal Recorder recorder;
        internal GameObject recorderGameObject;
        internal RecorderComponent recorderComponent;
        
        int m_FrameIndex = 0;
        int m_InitialFrame = 0;
        int m_FirstRecordedFrameCount = -1;
        float m_FPSTimeStart;
        float m_FPSNextTimeStart;
        int m_FPSNextFrameCount;

        internal double currentFrameStartTS { get; private set; }
        internal double recordingStartTS { get; private set; }
        
        internal DateTime sessionStartTS { get; private set; }

        internal RecorderSettings settings
        {
            get { return recorder.settings; }
        }

        internal bool isRecording
        {
            get { return recorder.recording; }
        }

        internal int frameIndex
        {
            get { return m_FrameIndex; }
        }

        internal int RecordedFrameSpan
        {
            get { return m_FirstRecordedFrameCount == -1 ? 0 : Time.renderedFrameCount - m_FirstRecordedFrameCount; }
        }

        internal float recorderTime
        {
            get { return (float)(currentFrameStartTS - settings.startTime); }
        }

        static void AllowInBackgroundMode()
        {
            if (!Application.runInBackground)
            {
                Application.runInBackground = true;
                if (Options.verboseMode)
                    Debug.Log("Recording sessions is enabling Application.runInBackground!");
            }
        }

        internal bool SessionCreated()
        {
            try
            {
                AllowInBackgroundMode();
                recordingStartTS = (Time.time / Time.timeScale);
                sessionStartTS = DateTime.Now;
                recorder.SessionCreated(this);
                return true;

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        internal bool BeginRecording()
        {
            try
            {
                if (!settings.isPlatformSupported)
                {
                    Debug.LogError(string.Format("Recorder {0} does not support current platform", recorder.GetType().Name));
                    return false;
                }

                AllowInBackgroundMode();

                recordingStartTS = (Time.time / Time.timeScale);
                recorder.SignalInputsOfStage(ERecordingSessionStage.BeginRecording, this);

                if (!recorder.BeginRecording(this))
                    return false;
                m_InitialFrame = Time.renderedFrameCount;
                m_FPSTimeStart = Time.unscaledTime;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        internal void EndRecording()
        {
            if (!isRecording)
                return;

            try
            {
                recorder.SignalInputsOfStage(ERecordingSessionStage.EndRecording, this);
                recorder.EndRecording(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        internal void RecordFrame()
        {
            try
            {
                recorder.SignalInputsOfStage(ERecordingSessionStage.NewFrameReady, this);
                if (!recorder.SkipFrame(this))
                {
                    recorder.RecordFrame(this);
                    recorder.recordedFramesCount++;
                    if (recorder.recordedFramesCount == 1)
                        m_FirstRecordedFrameCount = Time.renderedFrameCount;
                }
                recorder.SignalInputsOfStage(ERecordingSessionStage.FrameDone, this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            // Note: This is not great when multiple recorders are simultaneously active...
            if (settings.frameRatePlayback == FrameRatePlayback.Variable ||
                settings.frameRatePlayback == FrameRatePlayback.Constant && recorder.settings.capFrameRate)
            {
                var frameCount = Time.renderedFrameCount - m_InitialFrame;
                var frameLen = 1.0f / recorder.settings.frameRate;
                var elapsed = Time.unscaledTime - m_FPSTimeStart;
                var target = frameLen * (frameCount + 1);
                var sleep = (int)((target - elapsed) * 1000);

                if (sleep > 2)
                {
                    if (Options.verboseMode)
                        Debug.Log(string.Format("Recording session info => dT: {0:F1}s, Target dT: {1:F1}s, Retarding: {2}ms, fps: {3:F1}", elapsed, target, sleep, frameCount / elapsed));
                    System.Threading.Thread.Sleep(Math.Min(sleep, 1000));
                }
                else if (sleep < -frameLen)
                    m_InitialFrame--;
                else if (Options.verboseMode)
                    Debug.Log(string.Format("Recording session info => fps: {0:F1}", frameCount / elapsed));

                // reset every 30 frames
                if (frameCount % 50 == 49)
                {
                    m_FPSNextTimeStart = Time.unscaledTime;
                    m_FPSNextFrameCount = Time.renderedFrameCount;
                }
                if (frameCount % 100 == 99)
                {
                    m_FPSTimeStart = m_FPSNextTimeStart;
                    m_InitialFrame = m_FPSNextFrameCount;
                }
            }
            m_FrameIndex++;
        }

        internal void PrepareNewFrame()
        {
            try
            {
                AllowInBackgroundMode();

                currentFrameStartTS = (Time.time / Time.timeScale) - recordingStartTS;
                recorder.SignalInputsOfStage(ERecordingSessionStage.NewFrameStarting, this);
                recorder.PrepareNewFrame(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void Dispose()
        {
            if (recorder != null)
            {
                EndRecording();

                UnityHelpers.Destroy(recorder);
            }
        }
    }
}
