#if UNITY_2018_3_OR_NEWER
using UnityEditor.Animations;
#else
using UnityEditor.Experimental.Animations;
#endif

namespace UnityEditor.Recorder.Input
{
    class AnimationInput : RecorderInput
    {
        public GameObjectRecorder gameObjectRecorder { get; private set; }
        float m_Time;

        public override void BeginRecording(RecordingSession session)
        {
            var aniSettings = (AnimationInputSettings) settings;

            var srcGO = aniSettings.gameObject;

            if (srcGO == null)
                return;
            
            gameObjectRecorder = new GameObjectRecorder(srcGO);

            foreach (var binding in aniSettings.bindingType)
            {
                gameObjectRecorder.BindComponentsOfType(srcGO, binding, aniSettings.recursive); 
            }
            
            m_Time = session.recorderTime;
        }

        public override void NewFrameReady(RecordingSession session)
        {
            if (gameObjectRecorder != null && session.isRecording)
            {
                gameObjectRecorder.TakeSnapshot(session.recorderTime - m_Time);
                m_Time = session.recorderTime;
            }
        }        
    }
}