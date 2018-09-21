using UnityEngine;

namespace UnityEditor.Recorder
{
    abstract class GenericRecorder<T> : Recorder where T : RecorderSettings
    {
        [SerializeField]
        protected T m_Settings;
        
        public override RecorderSettings settings
        {
            get { return m_Settings; }
            set { m_Settings = (T)value; }
        }
    }
}
