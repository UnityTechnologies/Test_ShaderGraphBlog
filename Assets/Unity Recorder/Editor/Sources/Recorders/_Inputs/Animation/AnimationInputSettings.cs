using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    /// <inheritdoc/>
    /// <summary>
    /// Regroups all the information required to record an Animation from a given gameObject.
    /// </summary>
    [Serializable]
    [DisplayName("Animation")]
    public class AnimationInputSettings : RecorderInputSettings
    {
        [SerializeField] string m_BindingId = null;
        
        /// <summary>
        /// The gameObject to record from.
        /// </summary>
        public GameObject gameObject
        {
            get
            {
                if (string.IsNullOrEmpty(m_BindingId))
                    return null;
                
                return BindingManager.Get(m_BindingId) as GameObject;
            }

            set
            {
                if (string.IsNullOrEmpty(m_BindingId))
                    m_BindingId = GenerateBindingId();
                
                BindingManager.Set(m_BindingId, value);
            }
        }
        
        /// <summary>
        /// If true, all the gameObject hierarchy will be recorded.
        /// </summary>
        public bool recursive = true;
        
        /// <summary>
        /// Add a component to record from.
        /// </summary>
        /// <param name="componentType">The type of the Component</param>
        public void AddComponentToRecord(Type componentType)
        {
            if (componentType == null)
                return;

            var typeName = componentType.AssemblyQualifiedName;
            if (!bindingTypeNames.Contains(typeName))
                bindingTypeNames.Add(typeName);
        }
        
        [SerializeField]
        internal List<string> bindingTypeNames = new List<string>();
        
        internal List<Type> bindingType
        {
            get
            {
                var ret = new List<Type>(bindingTypeNames.Count);
                foreach (var t in bindingTypeNames)
                {
                    ret.Add(Type.GetType(t));
                }
                return ret;
            }
        }

        internal override Type inputType
        {
            get { return typeof(AnimationInput); }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (bindingType.Count > 0 && bindingType.Any(x => typeof(MonoBehaviour).IsAssignableFrom(x) || typeof(ScriptableObject).IsAssignableFrom(x))
            )
            {
                ok = false;
                errors.Add("MonoBehaviours and ScriptableObjects are not supported inputs.");
            }

            return ok;
        }

        static string GenerateBindingId()
        {
            return GUID.Generate().ToString();
        }

        internal void DuplicateExposedReference()
        {
            if (string.IsNullOrEmpty(m_BindingId))
                return;

            var src = m_BindingId;
            var dst = GenerateBindingId();

            m_BindingId = dst;
            
            BindingManager.Duplicate(src, dst);
        }

        internal void ClearExposedReference()
        {
            if (string.IsNullOrEmpty(m_BindingId))
                return;
            
            BindingManager.Set(m_BindingId, null);
        }
    }
}
