using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class AnimationRecorder : GenericRecorder<AnimationRecorderSettings>
    {
        public override void RecordFrame(RecordingSession session)
        {
        }

        public override void EndRecording(RecordingSession session)
        {
            var ars = (AnimationRecorderSettings)session.settings;

            foreach (var input in m_Inputs)
            {
                var aInput = (AnimationInput)input;

                if (aInput.gameObjectRecorder == null)
                    continue;
                
                var clip = new AnimationClip();
                
                ars.fileNameGenerator.CreateDirectory(session);

                var absolutePath = FileNameGenerator.SanitizePath(ars.fileNameGenerator.BuildAbsolutePath(session));
                var clipName = absolutePath.Replace(FileNameGenerator.SanitizePath(Application.dataPath), "Assets");
                
                AssetDatabase.CreateAsset(clip, clipName);
    #if UNITY_2018_3_OR_NEWER
                aInput.gameObjectRecorder.SaveToClip(clip, ars.frameRate);
    #else
                aInput.gameObjectRecorder.SaveToClip(clip);
    #endif
                aInput.gameObjectRecorder.ResetRecording();
            }

            base.EndRecording(session);
        }
    }
}