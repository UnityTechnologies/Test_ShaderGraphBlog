using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(ImageRecorderSettings))]
    class ImageRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_CaptureAlpha;

        static class Styles
        {
            internal static readonly GUIContent FormatLabel = new GUIContent("Format");
            internal static readonly GUIContent CaptureAlphaLabel = new GUIContent("Capture Alpha");
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.outputFormat);
            
            m_OutputFormat = serializedObject.FindProperty("outputFormat");
            m_CaptureAlpha = serializedObject.FindProperty("captureAlpha");
        }

        protected override void FileTypeAndFormatGUI()
        {           
            EditorGUILayout.PropertyField(m_OutputFormat, Styles.FormatLabel);

            var imageSettings = (ImageRecorderSettings) target;
            var outputFormat = imageSettings.outputFormat; 
            if (outputFormat == ImageRecorderOutputFormat.PNG || outputFormat == ImageRecorderOutputFormat.EXR)
            {
                var supportsAlpha = imageSettings.imageInputSettings.supportsTransparent; 
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
