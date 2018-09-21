using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    abstract class InputSettingsSelector
    {
        [SerializeField] string m_Selected;
        
        readonly Dictionary<string, RecorderInputSettings> m_RecorderInputSettings = new Dictionary<string, RecorderInputSettings>();
        
        public RecorderInputSettings selected
        {
            get
            {
                if (string.IsNullOrEmpty(m_Selected) || !m_RecorderInputSettings.ContainsKey(m_Selected))
                    m_Selected = m_RecorderInputSettings.Keys.First();
                
                return m_RecorderInputSettings[m_Selected];
            }

            protected set
            {
                foreach (var field in InputSettingFields())
                {
                    var input = (RecorderInputSettings)field.GetValue(this);

                    if (input.GetType() == value.GetType())
                    {
                        field.SetValue(this, value);
                        m_Selected = field.Name;
                        m_RecorderInputSettings[m_Selected] = value;
                        break;
                    }
                }
            }
        }

        public IEnumerable<FieldInfo> InputSettingFields()
        {
            return GetInputFields(GetType()).Where(f => typeof(RecorderInputSettings).IsAssignableFrom(f.FieldType));
        }

        public static IEnumerable<FieldInfo> GetInputFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        protected InputSettingsSelector()
        {
            foreach (var field in InputSettingFields())
            {
                var input = (RecorderInputSettings)field.GetValue(this);
                m_RecorderInputSettings.Add(field.Name, input);
            }
        }
    }
}