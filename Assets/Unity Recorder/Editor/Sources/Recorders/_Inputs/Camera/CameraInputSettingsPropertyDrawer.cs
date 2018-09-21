using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(CameraInputSettings))]
    class CameraInputSettingsPropertyDrawer : InputPropertyDrawer<CameraInputSettings>
    {
        static ImageSource m_SupportedSources = ImageSource.MainCamera | ImageSource.ActiveCamera | ImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;

        SerializedProperty m_Source;
        SerializedProperty m_CameraTag;
        SerializedProperty m_FlipFinalOutput;
        SerializedProperty m_IncludeUI;

        SerializedProperty m_OutputResolution;
        
        bool m_Initialized;
        
        static class Styles
        {
            internal static readonly GUIContent SourceLabel  = new GUIContent("Source");
            internal static readonly GUIContent TagLabel  = new GUIContent("Tag");
            internal static readonly GUIContent IncludeUILabel = new GUIContent("Include UI");
            internal static readonly GUIContent FlipVerticalLabel = new GUIContent("Flip Vertical");
        }

        protected override void Initialize(SerializedProperty property)
        {
            if (m_Initialized)
                return;

            base.Initialize(property);
            
            m_Source = property.FindPropertyRelative("source");
            m_CameraTag = property.FindPropertyRelative("cameraTag");
            m_OutputResolution = property.FindPropertyRelative("m_OutputResolution");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            m_IncludeUI = property.FindPropertyRelative("captureUI");

            m_Initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (m_MaskedSourceNames == null)
                    m_MaskedSourceNames = EnumHelper.MaskOutEnumNames<ImageSource>((int)m_SupportedSources);
                
                var index = EnumHelper.GetMaskedIndexFromEnumValue<ImageSource>(m_Source.intValue, (int)m_SupportedSources);
                index = EditorGUILayout.Popup(Styles.SourceLabel, index, m_MaskedSourceNames);

                if (check.changed)
                    m_Source.intValue = EnumHelper.GetEnumValueFromMaskedIndex<ImageSource>(index, (int)m_SupportedSources);
            }

            var inputType = (ImageSource)m_Source.intValue;
            if ((ImageSource)m_Source.intValue == ImageSource.TaggedCamera )
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CameraTag, Styles.TagLabel);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(m_OutputResolution);

            if(inputType == ImageSource.ActiveCamera)
            {
                EditorGUILayout.PropertyField(m_IncludeUI, Styles.IncludeUILabel);
            }
            
            EditorGUILayout.PropertyField(m_FlipFinalOutput, Styles.FlipVerticalLabel);
        }
    }
}
