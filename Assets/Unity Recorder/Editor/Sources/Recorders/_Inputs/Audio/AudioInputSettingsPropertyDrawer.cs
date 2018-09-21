using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(AudioInputSettings))]
    class AudioInputSettingsPropertyDrawer : InputPropertyDrawer<AudioInputSettings>
    {
        SerializedProperty m_PreserveAudio;
	    protected override void Initialize(SerializedProperty property)
        {
            m_PreserveAudio = property.FindPropertyRelative("preserveAudio");

        }

	    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	    {
		    Initialize(property);
		    
            EditorGUILayout.PropertyField(m_PreserveAudio, new GUIContent("Capture audio"));

        }
    }
}