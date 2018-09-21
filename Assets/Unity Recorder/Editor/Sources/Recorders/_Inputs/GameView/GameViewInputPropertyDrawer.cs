using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(GameViewInputSettings))]
    class GameViewInputPropertyDrawer : InputPropertyDrawer<GameViewInputSettings>
    {
        SerializedProperty m_OutputResolution;

        protected override void Initialize(SerializedProperty property)
        {
            if (target != null)
                return;
            
            base.Initialize(property);
            
            m_OutputResolution = property.FindPropertyRelative("m_OutputResolution");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUILayout.PropertyField(m_OutputResolution);
        }
    }
}