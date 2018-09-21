using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(MovieRecorderSettings))]
    class MovieRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_EncodingBitRateMode;
        SerializedProperty m_CaptureAlpha;

        static class Styles
        {
            internal static readonly GUIContent VideoBitRateLabel = new GUIContent("Quality");   
            internal static readonly GUIContent FormatLabel = new GUIContent("Format");
            internal static readonly GUIContent CaptureAlphaLabel = new GUIContent("Capture Alpha");
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            m_OutputFormat = serializedObject.FindProperty("outputFormat");
            m_CaptureAlpha = serializedObject.FindProperty("captureAlpha");
            m_EncodingBitRateMode = serializedObject.FindProperty("videoBitRateMode");           
        }

        protected override void OnEncodingGui()
        {
           EditorGUILayout.PropertyField(m_EncodingBitRateMode, Styles.VideoBitRateLabel);
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, Styles.FormatLabel);

            var movieSettings = (MovieRecorderSettings) target;
            
            if (movieSettings.outputFormat == VideoRecorderOutputFormat.WEBM)
            {
                var supportsAlpha = movieSettings.imageInputSettings.supportsTransparent;
                
                if (!supportsAlpha)
                    m_CaptureAlpha.boolValue = false;

                using (new EditorGUI.DisabledScope(!supportsAlpha))
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(m_CaptureAlpha, Styles.CaptureAlphaLabel);
                    --EditorGUI.indentLevel;
                }
            }
        }
    }
}
