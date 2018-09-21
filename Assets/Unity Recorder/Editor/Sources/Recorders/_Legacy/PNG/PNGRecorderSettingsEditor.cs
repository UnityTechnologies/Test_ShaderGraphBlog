using UnityEngine;

namespace UnityEditor.Recorder.FrameCapturer
{
    [CustomEditor(typeof(PNGRecorderSettings))]
    class PNGRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PngEncoderSettings"), new GUIContent("Encoding"), true);
        }
    }
}
