using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Recorder;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{   
    static class UnityHelpers
    {
        internal static void Destroy(UnityObject obj, bool allowDestroyingAssets = false)
        {
            if (obj == null)
                return;
            
            if (EditorApplication.isPlaying)
                UnityObject.Destroy(obj);
            else
                UnityObject.DestroyImmediate(obj, allowDestroyingAssets);
        }

        internal static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }

        internal static GameObject CreateRecorderGameObject(string name)
        {
            var gameObject = new GameObject(name) { tag = "EditorOnly" };
            SetGameObjectVisibility(gameObject, Options.showRecorderGameObject);
            return gameObject;
        }

        public static void SetGameObjectsVisibility(bool value)
        {
            var rcb = BindingManager.FindRecorderBindings();
            foreach (var rc in rcb)
            {
                SetGameObjectVisibility(rc.gameObject, value);
            }
            
            var rcs = Object.FindObjectsOfType<RecorderComponent>();
            foreach (var rc in rcs)
            {
                SetGameObjectVisibility(rc.gameObject, value);
            }
        }
        
        static void SetGameObjectVisibility(GameObject obj, bool visible)
        {
            if (obj != null)
            {
                obj.hideFlags = visible ? HideFlags.None : HideFlags.HideInHierarchy;

                if (!Application.isPlaying)
                {
                    try
                    {
                        EditorSceneManager.MarkSceneDirty(obj.scene);
                        EditorApplication.RepaintHierarchyWindow();
                        EditorApplication.DirtyHierarchyWindowSorting();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
