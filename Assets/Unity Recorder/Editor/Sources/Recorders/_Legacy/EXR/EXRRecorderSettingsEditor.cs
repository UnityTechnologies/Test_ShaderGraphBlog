using UnityEngine;

namespace UnityEditor.Recorder.FrameCapturer
{
    [CustomEditor(typeof(EXRRecorderSettings))]
    class EXRRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExrEncoderSettings"), new GUIContent("Encoding"), true);
        }

    }
}
