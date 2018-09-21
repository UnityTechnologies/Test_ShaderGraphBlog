using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    [RecorderSettings(typeof(AnimationRecorder), "Animation Clip", "animation_recorder")]
    public class AnimationRecorderSettings : RecorderSettings
    {
        [SerializeField] AnimationInputSettings m_AnimationInputSettings = new AnimationInputSettings();

        public AnimationInputSettings animationInputSettings
        {
            get { return m_AnimationInputSettings; }
            set { m_AnimationInputSettings = value; }
        }
   
        public AnimationRecorderSettings()
        {
            var goWildcard = DefaultWildcard.GeneratePattern("GameObject");
           
            fileNameGenerator.AddWildcard(goWildcard, GameObjectNameResolver);
            fileNameGenerator.AddWildcard(DefaultWildcard.GeneratePattern("GameObjectScene"), GameObjectSceneNameResolver);

            fileNameGenerator.forceAssetsFolder = true;
            fileNameGenerator.root = OutputPath.Root.AssetsFolder;
            fileNameGenerator.fileName = "animation_" + goWildcard + "_" + DefaultWildcard.Take;            
        }

        string GameObjectNameResolver(RecordingSession session)
        {
            var go = m_AnimationInputSettings.gameObject;
            return go != null ? go.name : "None";
        }
        
        string GameObjectSceneNameResolver(RecordingSession session)
        {
            var go = m_AnimationInputSettings.gameObject;
            return go != null ? go.scene.name : "None";
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.WindowsEditor;
            }
        }

        public override IEnumerable<RecorderInputSettings> inputsSettings
        {
            get { yield return m_AnimationInputSettings; }
        }

        public override string extension
        {
            get { return "anim"; }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if (m_AnimationInputSettings.gameObject == null)
            {
                ok = false;
                errors.Add("No input object set");
            }

            return ok; 
        }
        
        public override void OnAfterDuplicate()
        {
            m_AnimationInputSettings.DuplicateExposedReference();
        }
        
        void OnDestroy()
        {
            m_AnimationInputSettings.ClearExposedReference();
        }
    }
}