using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityObject = UnityEngine.Object;

namespace UnityEngine.Recorder
{     
    /// <summary>
    /// Helper component mainly used to save Recorder's GameObject references.
    /// Some recorders (like the Animation Recorder) requires a GameObject reference from a Scene to record from.
    /// In this cases, this component is automatically added to the Scene and binds the selected GameObject to the recorder settings.
    /// </summary>
    [ExecuteInEditMode]
    public class RecorderBindings : MonoBehaviour
    {
        [Serializable]
        class PropertyObjects : SerializedDictionary<string, UnityObject> { }
        
        [SerializeField] PropertyObjects m_References = new PropertyObjects();
        
        public void SetBindingValue(string id, UnityObject value)
        {
#if UNITY_EDITOR
            var dirty = !m_References.dictionary.ContainsKey(id) || m_References.dictionary[id] != value;
#endif
            m_References.dictionary[id] = value;
            
#if UNITY_EDITOR
            if (dirty)
                MarkSceneDirty();
#endif
        }
        
        public UnityObject GetBindingValue(string id)
        {
            UnityObject value;
            return m_References.dictionary.TryGetValue(id, out value) ? value : null;
        }
        
        public bool HasBindingValue(string id)
        {
            return m_References.dictionary.ContainsKey(id);
        }

        public void RemoveBinding(string id)
        {
            if (m_References.dictionary.ContainsKey(id))
            {
                m_References.dictionary.Remove(id);
                
                MarkSceneDirty();
            }
        }

        public bool IsEmpty()
        {
            return m_References == null || !m_References.dictionary.Keys.Any();
        }

        public void DuplicateBinding(string src, string dst)
        {
            if (m_References.dictionary.ContainsKey(src))
            {
                m_References.dictionary[dst] = m_References.dictionary[src];

                MarkSceneDirty();
            }
        }

        void MarkSceneDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }
    }
}