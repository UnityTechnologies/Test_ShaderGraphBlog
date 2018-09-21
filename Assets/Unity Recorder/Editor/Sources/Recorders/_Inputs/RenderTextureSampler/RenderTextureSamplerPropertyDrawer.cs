using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(RenderTextureSamplerSettings))]
    class RenderTextureSamplerPropertyDrawer : InputPropertyDrawer<RenderTextureSamplerSettings>
    {
        static ImageSource m_SupportedSources = ImageSource.ActiveCamera | ImageSource.MainCamera | ImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;
        SerializedProperty m_Source;
        SerializedProperty m_RenderHeight;
        SerializedProperty m_FinalHeight;
        SerializedProperty m_AspectRatio;
        SerializedProperty m_SuperSampling;
        SerializedProperty m_CameraTag;
        SerializedProperty m_FlipFinalOutput;

        ImageHeightSelector m_RenderHeightSelector;
        ImageHeightSelector m_FinalHeightSelector;

        ImageHeight m_RenderSizeEnum;
        ImageHeight m_FinalSizeEnum;
        
        static class Styles
        {
            internal static readonly GUIContent TagLabel =new GUIContent("Tag");
            internal static readonly GUIContent AspectRatioLabel = new GUIContent("Aspect Ratio");
            internal static readonly GUIContent SuperSamplingLabel = new GUIContent("Super Sampling");
            internal static readonly GUIContent RenderingResolutionLabel = new GUIContent("Rendering Resolution");
            internal static readonly GUIContent OutputResolutionLabel = new GUIContent("Output Resolution");
            internal static readonly GUIContent FlipVerticalLabel = new GUIContent("Flip Vertical");
        }

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);

            m_Source = property.FindPropertyRelative("source");
            m_RenderHeight = property.FindPropertyRelative("m_RenderHeight");
            m_FinalHeight = property.FindPropertyRelative("m_OutputHeight");
            m_AspectRatio = property.FindPropertyRelative("outputAspectRatio");
            m_SuperSampling = property.FindPropertyRelative("superSampling");
            m_CameraTag = property.FindPropertyRelative("cameraTag");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            
            m_RenderHeightSelector = new ImageHeightSelector((int)ImageHeight.x4320p_8K, false, false);
            m_FinalHeightSelector = new ImageHeightSelector((int)ImageHeight.x4320p_8K, false, false);
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

            if ((ImageSource)m_Source.intValue == ImageSource.TaggedCamera)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CameraTag, Styles.TagLabel);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_AspectRatio, Styles.AspectRatioLabel);
            EditorGUILayout.PropertyField(m_SuperSampling, Styles.SuperSamplingLabel);

            m_RenderHeight.intValue = m_RenderHeightSelector.Popup(Styles.RenderingResolutionLabel, m_RenderHeight.intValue, (int)ImageHeight.x4320p_8K);
            
            if (m_FinalHeight.intValue > m_RenderHeight.intValue)
                m_FinalHeight.intValue = m_RenderHeight.intValue;
            
            m_FinalHeight.intValue = m_FinalHeightSelector.Popup(Styles.OutputResolutionLabel, m_FinalHeight.intValue, target.kMaxSupportedSize);
            
            if (m_FinalHeight.intValue > m_RenderHeight.intValue)
                m_RenderHeight.intValue = m_FinalHeight.intValue;
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_FlipFinalOutput, Styles.FlipVerticalLabel);
            
            if (Options.verboseMode)
                EditorGUILayout.LabelField("Color Space", target.colorSpace.ToString());
            
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                
                var aspect = target.outputAspectRatio.GetAspect();
                target.outputWidth = (int)(aspect * target.outputHeight);
                target.renderWidth = (int)(aspect * target.renderHeight);
            }

        }
    }

}
