using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(AnimationInputSettings))]
    class AnimationInputSettingsPropertyDrawer : InputPropertyDrawer<AnimationInputSettings>
    {
        SerializedProperty m_Recursive;
        
        protected override void Initialize(SerializedProperty prop)
        {
            base.Initialize(prop);

            m_Recursive = prop.FindPropertyRelative("recursive");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
                              
            EditorGUI.BeginChangeCheck();
            
            var gameObject = EditorGUILayout.ObjectField("Game Object", target.gameObject, typeof(GameObject), true) as GameObject;
            
            if (EditorGUI.EndChangeCheck())
            {
                target.gameObject = gameObject;
                
                if (gameObject != null)
                    target.AddComponentToRecord(gameObject.GetComponent<Component>().GetType());
            }                     
            
            if (gameObject != null)
            {
                var compos = gameObject.GetComponents<Component>()
                    .Where(x => x != null)
                    .Select(x => x.GetType());
                if (target.recursive)
                {
                    compos = compos.Union(gameObject.GetComponentsInChildren<Component>()
                        .Where(x => x != null)
                        .Select(x => x.GetType()));
                }
                
                var distinctCompos = compos.Distinct()
                    .Where(x => !typeof(MonoBehaviour).IsAssignableFrom(x) && x != typeof(Animator)) // black list
                    .ToList();
                var compoNames = distinctCompos.Select(x => x.AssemblyQualifiedName).ToList();

                var flags = 0;
                foreach (var t in target.bindingTypeNames)
                {
                    var found = compoNames.IndexOf(t);
                    if (found != -1)
                        flags |= 1 << found;
                }
                
                EditorGUI.BeginChangeCheck();
                
                flags = EditorGUILayout.MaskField("Recorded Target(s)", flags, distinctCompos.Select(x => x.Name).ToArray());
                
                if (EditorGUI.EndChangeCheck())
                {
                    target.bindingTypeNames = new List<string>();
                    for (int i=0; i<compoNames.Count; ++i)                               
                    {
                        if ((flags & (1 << i )) == 1 << i )
                        {
                            target.bindingTypeNames.Add(compoNames[i]);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(m_Recursive, new GUIContent("Record Hierarchy"));   
        }
    }
}