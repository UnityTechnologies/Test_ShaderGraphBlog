using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(AspectRatio))]
    class AspectRatioPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_CustomAspectX;
        SerializedProperty m_CustomAspectY;
        
        SerializedProperty m_ImageAspect;

        bool m_Initialized;

        const string k_Format = "0.0###";
        static GUIContent[] s_DisplayNames;
        
        static readonly Dictionary<ImageAspect, string> s_AspectToName = new Dictionary<ImageAspect, string>
        {
            { ImageAspect.x19_10, "19:10 (" + AspectRatio.s_AspectToValue[ImageAspect.x19_10].ToString(k_Format) + ")"},
            { ImageAspect.x16_9, "16:9 (" + AspectRatio.s_AspectToValue[ImageAspect.x16_9].ToString(k_Format) + ")"},
            { ImageAspect.x16_10, "16:10 (" + AspectRatio.s_AspectToValue[ImageAspect.x16_10].ToString(k_Format) + ")"},
            { ImageAspect.x5_4, "5:4 (" + AspectRatio.s_AspectToValue[ImageAspect.x5_4].ToString(k_Format) + ")"},
            { ImageAspect.x4_3, "4:3 (" + AspectRatio.s_AspectToValue[ImageAspect.x4_3].ToString(k_Format) + ")"},
            { ImageAspect.x3_2, "3:2 (" + AspectRatio.s_AspectToValue[ImageAspect.x3_2].ToString(k_Format) + ")"},
            { ImageAspect.x1_1, "1:1 (" + AspectRatio.s_AspectToValue[ImageAspect.x1_1].ToString(k_Format) + ")"},
            { ImageAspect.Custom, "Custom"}
        };

        void Initialize(SerializedProperty property)
        {
            if (m_Initialized )
                return;

            m_Initialized = true;
                
            m_CustomAspectX = property.FindPropertyRelative("m_CustomAspectX");
            m_CustomAspectY = property.FindPropertyRelative("m_CustomAspectY");
        
            m_ImageAspect = property.FindPropertyRelative("m_ImageAspect");

            if (s_DisplayNames == null)
            {
                s_DisplayNames = ((ImageAspect[]) Enum.GetValues(typeof(ImageAspect))).Select(e => new GUIContent(s_AspectToName[e])).ToArray();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginProperty(position, label, property);
            
            m_ImageAspect.intValue = EditorGUI.Popup(position, label, m_ImageAspect.intValue, s_DisplayNames);
                
            EditorGUI.EndProperty();

            var aspect = (ImageAspect) m_ImageAspect.intValue;
            if (aspect == ImageAspect.Custom)
            {
                CustomAspectField();
            }
        }
        
        void CustomAspectField()
        {
            var r = EditorGUILayout.GetControlRect();

            var rContent = r;
            rContent.xMin = r.x + EditorGUIUtility.labelWidth;
            
            EditorGUI.BeginChangeCheck();

            int indentLevel = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;

            const float columnWidth = 12.0f;
            var w = Mathf.Max(30.0f, (rContent.width - columnWidth) / 3.0f);

            var rCurrent = rContent;
            rCurrent.width = w;
            
            var x = EditorGUI.FloatField(rCurrent, m_CustomAspectX.floatValue);

            rCurrent.x += w;
            rCurrent.width = columnWidth;
            EditorGUI.LabelField(rCurrent, ":");
            
            rCurrent.x += columnWidth;
            rCurrent.width = w;
            var y  = EditorGUI.FloatField(rCurrent, m_CustomAspectY.floatValue );

            if (EditorGUI.EndChangeCheck())
            {
                m_CustomAspectX.floatValue = Mathf.Max(x, 1.0f);
                m_CustomAspectY.floatValue = Mathf.Max(y, 1.0f);
            }

            var aspect = m_CustomAspectX.floatValue / m_CustomAspectY.floatValue;

            rCurrent.xMin = rCurrent.xMax + 3.0f;
            rCurrent.xMax = r.xMax;
            EditorGUI.LabelField(rCurrent, "(" + aspect.ToString(k_Format) + ")");
            
            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}