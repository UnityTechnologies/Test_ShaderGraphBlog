using System.Collections.Generic;

namespace UnityEditor.Recorder
{  
    [CustomEditor(typeof(RecorderControllerSettingsPreset))]
    class RecorderControllerSettingsPresetEditor : Editor
    {
        Editor m_Editor;

        class PresetEditorState
        {
            public bool expanded;
            public Editor presetEditor;
        }
        
        readonly List<PresetEditorState> m_RecordersEditors = new List<PresetEditorState>();

        void OnEnable()
        {
            if (target == null)
                return;
            
            var preset = (RecorderControllerSettingsPreset) target;
            
            m_Editor = CreateEditor(preset.model);

            m_RecordersEditors.Clear();

            var recorderPresets = preset.recorderPresets;

            foreach (var p in recorderPresets)
            {
                var state = new PresetEditorState
                {
                    presetEditor = CreateEditor(p),
                    expanded = false
                };
                
                m_RecordersEditors.Add(state);
            }
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;
            
            m_Editor.OnInspectorGUI();
            
            EditorGUILayout.Separator();
            
            foreach (var state in m_RecordersEditors)
            {
                if (FoldoutPresetEditorStateHeader(state))
                {
                    EditorGUILayout.Separator();
                    state.presetEditor.OnInspectorGUI();
                }
            }            
        }

        static bool FoldoutPresetEditorStateHeader(PresetEditorState state)
        {
            var r = EditorGUILayout.GetControlRect();
            state.expanded = EditorGUI.Foldout(r, state.expanded, state.presetEditor.target.name);

            return state.expanded;
        }

        void OnDestroy()
        {
            if (m_Editor != null)
            {
                DestroyImmediate(m_Editor);
                m_Editor = null;
            }

            foreach (var state in m_RecordersEditors)
                DestroyImmediate(state.presetEditor);
            
            m_RecordersEditors.Clear();
        }
    }
}