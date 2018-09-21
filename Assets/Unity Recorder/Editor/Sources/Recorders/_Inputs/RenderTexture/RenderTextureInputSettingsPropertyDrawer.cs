using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(RenderTextureInputSettings))]
    class RenderTextureInputSettingsPropertyDrawer : InputPropertyDrawer<RenderTextureInputSettings>
    {
        SerializedProperty m_SourceRTxtr;
        SerializedProperty m_FlipFinalOutput;
        
        static class Styles
        {
            internal static readonly GUIContent RenderTextureLabel = new GUIContent("Render Texture");
            internal static readonly GUIContent FlipVerticalLabel = new GUIContent("Flip Vertical");
        }
        
        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);

            if (m_SourceRTxtr == null)
                m_SourceRTxtr = property.FindPropertyRelative("renderTexture");
            
            if (m_FlipFinalOutput == null)
                m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            EditorGUILayout.PropertyField(m_SourceRTxtr, Styles.RenderTextureLabel);

            var res = "N/A";
            if (m_SourceRTxtr.objectReferenceValue != null)
            {
                var renderTexture = (RenderTexture)m_SourceRTxtr.objectReferenceValue;
                res = string.Format("{0}x{1}", renderTexture.width, renderTexture.height);
            }
            EditorGUILayout.LabelField("Resolution", res);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_FlipFinalOutput, Styles.FlipVerticalLabel);
        }
    }
}