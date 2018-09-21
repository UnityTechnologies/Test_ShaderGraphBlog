using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder
{
    abstract class EnumProperyDrawer<T> : PropertyDrawer
    {
        GUIContent[] m_DisplayNames;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_DisplayNames == null)
            {
                var displayNames = new List<GUIContent>();

                foreach (T frameRate in Enum.GetValues(typeof(T)))
                {
                    displayNames.Add(new GUIContent(ToLabel(frameRate)));
                }

                m_DisplayNames = displayNames.ToArray();
            }
            
            EditorGUI.BeginProperty(position, label, property);
            
            property.intValue = EditorGUI.Popup(position, label, property.intValue, m_DisplayNames);
                
            EditorGUI.EndProperty();
        }

        protected abstract string ToLabel(T value); 
    }
}