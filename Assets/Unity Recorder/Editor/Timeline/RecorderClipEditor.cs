using System;
using System.Globalization;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Timeline;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder.Timeline
{
    [CustomEditor(typeof(RecorderClip), true)]
    class RecorderClipEditor : Editor
    {
        RecorderEditor m_Editor;
        TimelineAsset m_Timeline;
        RecorderSelector m_RecorderSelector;

        public void OnEnable()
        {
            m_RecorderSelector = null;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                if (target == null)
                    return;

                // Bug? work arround: on Stop play, Enable is not called.
                if (m_Editor != null && m_Editor.target == null)
                {
                    UnityHelpers.Destroy(m_Editor);
                    m_Editor = null;
                    m_RecorderSelector = null;
                }

                if (m_RecorderSelector == null)
                {
                    m_RecorderSelector = new RecorderSelector();
                    m_RecorderSelector.OnSelectionChanged += OnRecorderSelected;
                    m_RecorderSelector.Init(((RecorderClip) target).settings);
                }

                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                {
                    if (m_Timeline == null)
                        m_Timeline = FindTimelineAsset();

                    if (m_Timeline != null)
                    {
                        EditorGUILayout.LabelField("Frame Rate");
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Playback", FrameRatePlayback.Constant.ToString());
                        EditorGUILayout.LabelField("Target (Timeline FPS)", m_Timeline.editorSettings.fps.ToString(CultureInfo.InvariantCulture));
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Separator();
                    }

                    EditorGUILayout.BeginHorizontal();
                    
                    var clip = (RecorderClip) target;

                    if (clip.needsDuplication)
                    {
                        if (clip.settings != null)
                        {
                            clip.settings = Instantiate(clip.settings);
                            AssetDatabase.AddObjectToAsset(clip.settings, clip);
                        }

                        clip.needsDuplication = false;
                    }
                    
                    m_RecorderSelector.OnGui();

                    if (m_Editor != null)
                    {
                        if (GUILayout.Button(PresetHelper.presetIcon, PresetHelper.presetButtonStyle))
                        {
                            var settings = m_Editor.target as RecorderSettings;

                            if (settings != null)
                            {
                                var presetReceiver = CreateInstance<PresetHelper.PresetReceiver>();
                                presetReceiver.Init(settings, Repaint);

                                PresetSelector.ShowSelector(settings, null, true, presetReceiver);
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    if (m_Editor != null)
                    {
                        PushTimelineIntoRecorder();

                        EditorGUILayout.Separator();

                        m_Editor.OnInspectorGUI();

                        serializedObject.Update();
                    }
                }
            }
            catch (ExitGUIException)
            {
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox("An exception was raised while editing the settings. This can be indicative of corrupted settings.", MessageType.Warning);

                if (GUILayout.Button("Reset settings to default"))
                    ResetSettings();

                Debug.LogException(ex);
            }
        }

        void ResetSettings()
        {
            UnityHelpers.Destroy(m_Editor);
            m_Editor = null;
            m_RecorderSelector = null;
            UnityHelpers.Destroy(((RecorderClip) target).settings, true);
        }

        void OnRecorderSelected(Type selectedRecorder)
        {
            var clip = (RecorderClip)target;

            if (m_Editor != null)
            {
                UnityHelpers.Destroy(m_Editor);
                m_Editor = null;
            }

            if (selectedRecorder == null)
                return;

            if (clip.settings != null && RecordersInventory.GetRecorderInfo(selectedRecorder).settingsType != clip.settings.GetType())
            {
                UnityHelpers.Destroy(clip.settings, true);
                clip.settings = null;
            }

            if (clip.settings == null)
            {
                clip.settings = RecordersInventory.CreateDefaultRecorderSettings(selectedRecorder);
                AssetDatabase.AddObjectToAsset(clip.settings, clip);
            }

            m_Editor = (RecorderEditor) CreateEditor(clip.settings);
            AssetDatabase.Refresh();
        }

        TimelineAsset FindTimelineAsset()
        {
            if (!AssetDatabase.Contains(target))
                return null;

            var path = AssetDatabase.GetAssetPath(target);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objs)
            {
                if (obj != null && AssetDatabase.IsMainAsset(obj))
                    return obj as TimelineAsset;
            }
            return null;
        }

        void PushTimelineIntoRecorder()
        {
            if (m_Timeline == null)
                return;

            var settings = m_Editor.target as RecorderSettings;

            // Time
            if (settings != null)
            {
                settings.frameRate = m_Timeline.editorSettings.fps;
                settings.frameRatePlayback = FrameRatePlayback.Constant;
                settings.capFrameRate = true;
            }
        }
    }
}