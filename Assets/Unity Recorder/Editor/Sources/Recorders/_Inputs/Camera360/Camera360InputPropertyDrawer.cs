using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(Camera360InputSettings))]
    class Camera360InputPropertyDrawer : InputPropertyDrawer<Camera360InputSettings>
    {
        static ImageSource m_SupportedSources = ImageSource.MainCamera | ImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;

        SerializedProperty m_Source;
        SerializedProperty m_CameraTag;
        SerializedProperty m_FlipFinalOutput;
        SerializedProperty m_StereoSeparation;
        SerializedProperty m_CubeMapSz;
        SerializedProperty m_OutputWidth;
        SerializedProperty m_OutputHeight;
        SerializedProperty m_RenderStereo;
        
        static class Styles
        {
            static readonly GUIContent s_WidthLabel = new GUIContent("W");
            static readonly GUIContent s_HeightLabel = new GUIContent("H");
            
            internal static readonly GUIContent TagLabel  = new GUIContent("Tag");
            internal static readonly GUIContent OutputLabel = new GUIContent("360 View Output");
            internal static readonly GUIContent[] DimensionLabels = { s_WidthLabel, s_HeightLabel };
            internal static readonly GUIContent CubeMapLabel = new GUIContent("Cube Map");
            internal static readonly GUIContent[] CubeDimensionLabel = { s_WidthLabel };
            internal static readonly GUIContent StereoLabel = new GUIContent("Stereo");
            internal static readonly GUIContent StereoSeparationLabel = new GUIContent("Stereo Separation");
            internal static readonly GUIContent FlipVerticalLabel = new GUIContent("Flip Vertical");
        }

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);
            
            m_Source = property.FindPropertyRelative("source");
            m_CameraTag = property.FindPropertyRelative("cameraTag");

            m_StereoSeparation = property.FindPropertyRelative("stereoSeparation");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            m_CubeMapSz = property.FindPropertyRelative("mapSize");
            m_OutputWidth = property.FindPropertyRelative("m_OutputWidth");
            m_OutputHeight = property.FindPropertyRelative("m_OutputHeight");
            m_RenderStereo = property.FindPropertyRelative("renderStereo");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (m_MaskedSourceNames == null)
                    m_MaskedSourceNames = EnumHelper.MaskOutEnumNames<ImageSource>((int)m_SupportedSources);
                
                var index = EnumHelper.GetMaskedIndexFromEnumValue<ImageSource>(m_Source.intValue, (int)m_SupportedSources);
                index = EditorGUILayout.Popup("Source", index, m_MaskedSourceNames);

                if (check.changed)
                    m_Source.intValue = EnumHelper.GetEnumValueFromMaskedIndex<ImageSource>(index, (int)m_SupportedSources);
            }

            if ((ImageSource)m_Source.intValue == ImageSource.TaggedCamera )
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CameraTag, Styles.TagLabel);
                --EditorGUI.indentLevel;
            }
            
            var outputDimensions = new int[2];
            outputDimensions[0] = m_OutputWidth.intValue;
            outputDimensions[1] = m_OutputHeight.intValue;
            
            if (UIElementHelper.MultiIntField(Styles.OutputLabel, Styles.DimensionLabels, outputDimensions))
            {
                m_OutputWidth.intValue = outputDimensions[0];
                m_OutputHeight.intValue = outputDimensions[1];
            }
            
            var cubeMapWidth = new int[1];
            cubeMapWidth[0] = m_CubeMapSz.intValue;
            outputDimensions[1] = m_OutputHeight.intValue;
            
            if (UIElementHelper.MultiIntField(Styles.CubeMapLabel, Styles.CubeDimensionLabel, cubeMapWidth))
            {
                m_CubeMapSz.intValue = cubeMapWidth[0];
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_RenderStereo, Styles.StereoLabel);

            ++EditorGUI.indentLevel;
            using (new EditorGUI.DisabledScope(!m_RenderStereo.boolValue))
            {
                EditorGUILayout.PropertyField(m_StereoSeparation, Styles.StereoSeparationLabel);
            }
            --EditorGUI.indentLevel;
            
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_FlipFinalOutput, Styles.FlipVerticalLabel);
        }
    }
}