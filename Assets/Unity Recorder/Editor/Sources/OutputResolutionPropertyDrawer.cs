using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(OutputResolution))]
    class OutputResolutionPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_CustomWidth;
        SerializedProperty m_CustomHeight;
        
        SerializedProperty m_ImageHeight;
        SerializedProperty m_AspectRatio;
        
        SerializedProperty m_MaxSupportedHeight;

        ImageHeightSelector m_HeightSelector;

        bool m_Initialized;
        
        static class Styles
        {
            internal static readonly GUIContent ImageAspectLabel = new GUIContent("Aspect Ratio");
            
            static readonly GUIContent s_CustomWidthLabel = new GUIContent("W");
            static readonly GUIContent s_CustomHeightLabel = new GUIContent("H");

            internal static readonly GUIContent[] CustomDimensionsLabels = { s_CustomWidthLabel, s_CustomHeightLabel };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        void Initialize(SerializedProperty property)
        {
            if (m_Initialized )
                return;

            m_Initialized = true;
                
            m_CustomWidth = property.FindPropertyRelative("m_CustomWidth");
            m_CustomHeight = property.FindPropertyRelative("m_CustomHeight");
        
            m_ImageHeight = property.FindPropertyRelative("imageHeight");
            m_AspectRatio = property.FindPropertyRelative("m_AspectRatio");
            
            m_MaxSupportedHeight = property.FindPropertyRelative("maxSupportedHeight");
            
            m_HeightSelector = new ImageHeightSelector(m_MaxSupportedHeight.intValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            m_ImageHeight.intValue = m_HeightSelector.Popup(label, m_ImageHeight.intValue, m_MaxSupportedHeight.intValue);
            var selected = (ImageHeight) m_ImageHeight.intValue;

            if (selected == ImageHeight.Custom)
            {
                var outputDimensions = new int[2];
                outputDimensions[0] = m_CustomWidth.intValue;
                outputDimensions[1] = m_CustomHeight.intValue;
                
                if (UIElementHelper.MultiIntField(GUIContent.none, Styles.CustomDimensionsLabels, outputDimensions))
                {
                    m_CustomWidth.intValue = outputDimensions[0];
                    m_CustomHeight.intValue = outputDimensions[1];
                }
            }

            if (selected != ImageHeight.Custom && selected != ImageHeight.Window)
            {
                EditorGUILayout.PropertyField(m_AspectRatio, Styles.ImageAspectLabel);
            }
        }
        
        
    }
}