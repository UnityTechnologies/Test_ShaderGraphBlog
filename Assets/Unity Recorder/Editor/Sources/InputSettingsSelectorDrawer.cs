using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(InputSettingsSelector), true)]
    class InputSettingsSelectorDrawer : TargetedPropertyDrawer<InputSettingsSelector>
    {
        bool m_Initialized;
        
        GUIContent[] m_DisplayNames;
        Dictionary<string, int> m_NameToIndex;
        Dictionary<int, SerializedProperty> m_IndexToProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {           
            if (!m_Initialized)
            {
                Initialize(property);
                m_Initialized = true;
            }

            if (m_DisplayNames.Length == 0)
            {
                EditorGUILayout.LabelField("No input to select.");
                return;
            }

            var newIndex = 0;
            var selected = property.FindPropertyRelative("m_Selected");
            
            if (m_DisplayNames.Length > 1)
            {
                int index;

                m_NameToIndex.TryGetValue(selected.stringValue, out index);

                newIndex = EditorGUILayout.Popup(label, index, m_DisplayNames);
            }
            
            var sp = m_IndexToProperty[newIndex];
            selected.stringValue = sp.name;

            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(sp, true);
            --EditorGUI.indentLevel;
        }

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);
                
            m_NameToIndex = new Dictionary<string, int>();
            m_IndexToProperty = new Dictionary<int, SerializedProperty>();
            
            var displayNames = new List<GUIContent>();
            
            int i = 0;
            foreach (var field in target.InputSettingFields())
            {
                var sp = property.FindPropertyRelative(field.Name);
                
                m_NameToIndex.Add(sp.name, i);
                m_IndexToProperty.Add(i, sp);
                displayNames.Add(new GUIContent(GetTypeDisplayName(field.FieldType)));
                ++i;
            }

            m_DisplayNames = displayNames.ToArray();
        }
        
        static string GetTypeDisplayName(Type type)
        {
            var displayNameAttribute = type.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;

            return displayNameAttribute != null
                ? displayNameAttribute.DisplayName
                : ObjectNames.NicifyVariableName(type.Name);
        }
    }
}