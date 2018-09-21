using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.SceneManagement;

namespace UnityEditor.Recorder
{
    static class BindingManager
    {  
        const string k_HostGoName = "Unity-RecorderBindings";
        
        public static Object Get(string id)
        {
            var rcs = FindRecorderBindings();
            var rc = rcs.FirstOrDefault(r => r.HasBindingValue(id));
            
            return rc != null ? rc.GetBindingValue(id) : null;
        }

        public static void Duplicate(string id, string newId)
        {
            var rcs = FindRecorderBindings();
            foreach (var rc in rcs)
            {
                rc.DuplicateBinding(id, newId);
            }
        }
        
        public static void Set(string id, Object obj)
        {
            var rbs = FindRecorderBindings();

            if (obj == null)
            {
                // Remove
                foreach (var rb in rbs)
                {
                    RemoveBinding(id, rb);
                }
            }
            else
            {
                var scene = GetObjectScene(obj);

                var rb = rbs.FirstOrDefault(r => r.gameObject.scene == scene);

                if (rb == null)
                {
                    // Add
                    var gameObject = UnityHelpers.CreateRecorderGameObject(k_HostGoName);
                    rb = gameObject.AddComponent<RecorderBindings>();
                    SceneManager.MoveGameObjectToScene(rb.gameObject, scene);
                }
                
                // Replace
                rb.SetBindingValue(id, obj);

                foreach (var r in rbs)
                {
                    if (r == rb)
                        continue;
                    
                    RemoveBinding(id, r);
                }
            }
        }

        static void RemoveBinding(string id, RecorderBindings rb)
        {
            rb.RemoveBinding(id);
                    
            if (rb.IsEmpty())
                Object.DestroyImmediate(rb.gameObject);
        }
        
        public static RecorderBindings[] FindRecorderBindings()
        {
            return Object.FindObjectsOfType<RecorderBindings>();
        }

        static Scene GetObjectScene(Object obj)
        {
            var gameObject = obj as GameObject;
            if (gameObject != null)
                return gameObject.scene;

            var component = obj as Component;
            if (component != null)
                return component.gameObject.scene;
            
            return SceneManager.GetActiveScene();
        }
    }
    
    class SceneHook
    {
        const string k_HostGoName = "Unity-RecorderSessions";
        
        static GameObject s_SessionHooksRoot;

        readonly string m_SessionId;
        GameObject m_SessionHook;
        
        public SceneHook(string sessionId)
        {
            m_SessionId = sessionId;
        }

        static GameObject GetSessionHooksRoot(bool createIfNecessary = true)
        {
            if (s_SessionHooksRoot == null)
            {  
                s_SessionHooksRoot = GameObject.Find(k_HostGoName);

                if (s_SessionHooksRoot == null)
                {
                    if (!createIfNecessary)
                        return null;
                    
                    s_SessionHooksRoot = UnityHelpers.CreateRecorderGameObject(k_HostGoName);
                }
            }

            return s_SessionHooksRoot;
        }

        GameObject GetSessionHook()
        {
            if (m_SessionHook != null)
                return m_SessionHook;
            
            var host = GetSessionHooksRoot();
            if (host == null)
                return null;
            
            m_SessionHook = GameObject.Find(m_SessionId);
            if (m_SessionHook == null)
            {
                m_SessionHook = new GameObject(m_SessionId);
                m_SessionHook.transform.parent = host.transform;   
            }

            return m_SessionHook;
        }

        public IEnumerable<RecordingSession> GetRecordingSessions()
        {
            var sessionHook = GetSessionHook();
            if (sessionHook != null)
            {
                var components = sessionHook.GetComponents<RecorderComponent>();
                foreach (var component in components)
                {
                    yield return component.session;
                }      
            }
        }

        public static void PrepareSessionRoot()
        {
            var host = GetSessionHooksRoot();
            if (host != null)
            {
                host.hideFlags = HideFlags.None;
                Object.DontDestroyOnLoad(host);
            }
        }
        
        public RecordingSession CreateRecorderSessionWithRecorderComponent(RecorderSettings settings)
        {
            var component = GetRecorderComponent(settings);
            
            var session = new RecordingSession
            {
                recorder = RecordersInventory.CreateDefaultRecorder(settings),
                recorderGameObject = component.gameObject,
                recorderComponent = component
            };

            component.session = session;

            return session;
        }
        
        public RecordingSession CreateRecorderSession(RecorderSettings settings)
        {
            var sceneHook = GetSessionHook();
            if (sceneHook == null)
                return null;
            
            var session = new RecordingSession
            {
                recorder = RecordersInventory.CreateDefaultRecorder(settings),
                recorderGameObject = sceneHook
            };

            return session;
        }
        
        RecorderComponent GetRecorderComponent(RecorderSettings settings)
        {
            var sceneHook = GetSessionHook();
            if (sceneHook == null)
                return null;

            var component = sceneHook.GetComponentsInChildren<RecorderComponent>().FirstOrDefault(r => r.session.settings == settings);

            if (component == null)
                component = sceneHook.AddComponent<RecorderComponent>();

            return component;
        }
    }
}
