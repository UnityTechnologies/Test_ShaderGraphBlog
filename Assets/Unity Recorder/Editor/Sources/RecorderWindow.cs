using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{   
    /// <summary>
    /// Main window class of the Unity Recorder.
    /// It can be accessed from an Editor script to show the Recorder Window and eventually Start and Stop the recording using current settings.
    /// Recorder settings are saved in Library/Recorder/recorder.pref
    /// </summary>
    public class RecorderWindow : EditorWindow
    {
        static readonly string s_WindowTitle = "Recorder";
        static readonly string s_PrefsFileName = "/../Library/Recorder/recorder.pref";
        static readonly string s_StylesFolder = "Styles/";

    #if UNITY_2018_2_OR_NEWER
        public const string MenuRoot = "Window/General/Recorder/";
        public const int MenuRootIndex = 1000;
    #else
        public const string MenuRoot = "Window/Recorder/";
        public const int MenuRootIndex = 2050;
    #endif
        
        
        
        [MenuItem(MenuRoot + "Recorder Window", false, MenuRootIndex)]
        static void ShowRecorderWindow()
        {
            GetWindow(typeof(RecorderWindow), false, s_WindowTitle);
        }
        
        [MenuItem(MenuRoot + "Quick Recording _F10", false, MenuRootIndex + 1)]
        static void QuickRecording()
        {
            var recorderWindow = (RecorderWindow) GetWindow(typeof(RecorderWindow), false, s_WindowTitle, false);

            if (!recorderWindow.IsRecording())
            {
                recorderWindow.StartRecording();
            }
            else
            {
                recorderWindow.StopRecording();
            }
        }

        static class Styles
        {
            internal static readonly GUIContent ExitPlayModeLabel = new GUIContent("Exit PlayMode");
            internal static readonly GUIContent DuplicateLabel = new GUIContent("Duplicate");
            internal static readonly GUIContent DeleteLabel = new GUIContent("Delete");
            internal static readonly GUIContent SaveRecorderListLabel = new GUIContent("Save Recorder List");
            internal static readonly GUIContent LoadRecorderListLabel = new GUIContent("Load Recorder List");
            internal static readonly GUIContent ClearRecorderListLabel = new GUIContent("Clear Recorder List");
            
            internal static readonly GUIContent WaitingForPlayModeLabel = new GUIContent("Waiting for Playmode to start...");
        }

        class RecorderItemList : VisualListItem<RecorderItem>
        {
        }
        
        RecorderItemList m_RecordingListItem;
        
        VisualElement m_SettingsPanel;
        VisualElement m_RecordersPanel;
        
        RecorderItem m_SelectedRecorderItem;
        
        VisualElement m_ParametersControl;
        VisualElement m_RecorderSettingPanel;
        
        Button m_RecordButton;
        Button m_RecordButtonIcon;
        
        PanelSplitter m_PanelSplitter;
        VisualElement m_AddNewRecordPanel;
        
        VisualElement m_RecordOptionsPanel;
        VisualElement m_RecordModeOptionsPanel;
        VisualElement m_FrameRateOptionsPanel;
        
        RecorderControllerSettings m_ControllerSettings;
        RecorderController m_RecorderController;
        
        enum State
        {
            Idle,
            WaitingForPlayModeToStartRecording,
            Error,
            Recording
        }
        
        State m_State = State.Idle;
        int m_FrameCount = 0;

        RecorderSettingsPrefsEditor m_RecorderSettingsPrefsEditor;

        void OnEnable()
        {             
            minSize = new Vector2(560.0f, 200.0f);
            
            var root = this.GetRootVisualContainer();

            root.style.flexDirection = FlexDirection.Column; 
            
            root.AddStyleSheetPath(s_StylesFolder + "recorder");
            root.AddStyleSheetPath(s_StylesFolder + (EditorGUIUtility.isProSkin ? "recorder_darkSkin" : "recorder_lightSkin"));
            
            root.focusIndex = 0;

            var mainControls = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    minHeight = 110.0f 
                }
            };
            root.Add(mainControls);
            
            var controlLeftPane = new VisualElement
            {
                style =
                {
#if UNITY_2018_1
                    minWidth = 350.0f,
#else
                    minWidth = 180.0f,
#endif
                    maxWidth = 450.0f,
                    flexDirection = FlexDirection.Row,
                }
            };

            UIElementHelper.SetFlex(controlLeftPane, 0.5f);

            var controlRightPane = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                }
            };

            UIElementHelper.SetFlex(controlRightPane, 0.5f);
            
            mainControls.Add(controlLeftPane);
            mainControls.Add(controlRightPane);
            
            controlLeftPane.AddToClassList("StandardPanel");
            controlRightPane.AddToClassList("StandardPanel");

            m_RecordButtonIcon = new Button(OnRecordButtonClick)
            {
                name = "recorderIcon",
                style =
                {
                    backgroundImage = Resources.Load<Texture2D>("recorder_icon"),
                }
            };

            controlLeftPane.Add(m_RecordButtonIcon);


            var leftButtonsStack = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                }
            };

            UIElementHelper.SetFlex(leftButtonsStack, 1.0f);

            m_RecordButton = new Button(OnRecordButtonClick)
            {
                name = "recordButton"
            };
            
            UpdateRecordButtonText();

            leftButtonsStack.Add(m_RecordButton);

            m_RecordOptionsPanel = new IMGUIContainer(() =>
            {
                PrepareGUIState(m_RecordOptionsPanel.layout.width);
                Options.exitPlayMode = EditorGUILayout.Toggle(Styles.ExitPlayModeLabel, Options.exitPlayMode);
            })
            {
                name = "recordOptions"
            };

            UIElementHelper.SetFlex(m_RecordOptionsPanel, 1.0f);

            leftButtonsStack.Add(m_RecordOptionsPanel);
            
            m_RecordModeOptionsPanel = new IMGUIContainer(() =>
            {
                PrepareGUIState(m_RecordModeOptionsPanel.layout.width);
                if (m_RecorderSettingsPrefsEditor.RecordModeGUI())
                    OnGlobalSettingsChanged();
            });

            UIElementHelper.SetFlex(m_RecordModeOptionsPanel, 1.0f);

            leftButtonsStack.Add(m_RecordModeOptionsPanel);

            controlLeftPane.Add(leftButtonsStack);
            
            m_FrameRateOptionsPanel = new IMGUIContainer(() =>
            {
                PrepareGUIState(m_FrameRateOptionsPanel.layout.width);
                if (m_RecorderSettingsPrefsEditor.FrameRateGUI())
                    OnGlobalSettingsChanged();
            });

            UIElementHelper.SetFlex(m_FrameRateOptionsPanel, 1.0f);
            
            controlRightPane.Add(m_FrameRateOptionsPanel);

            m_SettingsPanel = new ScrollView();

            UIElementHelper.SetFlex(m_SettingsPanel, 1.0f);
            
            m_SettingsPanel.contentContainer.style.positionLeft = 0;
            m_SettingsPanel.contentContainer.style.positionRight = 0;

            var recordersAndParameters = new VisualElement
            {
                style =
                {
                    alignSelf = Align.Stretch,
                    flexDirection = FlexDirection.Row,
                }
            };

            UIElementHelper.SetFlex(recordersAndParameters, 1.0f);
            
            m_RecordersPanel = new VisualElement
            {
                name = "recordersPanel",
                style =
                {
                    width = 200.0f,
                    minWidth = 150.0f,
                    maxWidth = 500.0f
                }
            };
            
            m_RecordersPanel.AddToClassList("StandardPanel");

            m_PanelSplitter = new PanelSplitter(m_RecordersPanel)
            {
                persistenceKey = "RecordingPanelSplitter"
            };
            
            recordersAndParameters.Add(m_RecordersPanel);
            recordersAndParameters.Add(m_PanelSplitter);
            recordersAndParameters.Add(m_SettingsPanel);
            
            m_SettingsPanel.AddToClassList("StandardPanel");

            root.Add(recordersAndParameters);
            
            var addRecordButton = new Label("+ Add New Recorders");
            UIElementHelper.SetFlex(addRecordButton, 1.0f);    

            var recorderListPresetButton = new VisualElement
            {
                name = "recorderListPreset",                
            };
            
            recorderListPresetButton.RegisterCallback<MouseUpEvent>(evt => ShowRecorderListMenu());
            
            recorderListPresetButton.Add(new Image
            {
                image = (Texture2D)EditorGUIUtility.Load("Builtin Skins/" + (EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin") + "/Images/pane options.png"),
                style =
                {
                    width = 16.0f,
                    height = 11.0f
                }
            });
            
            addRecordButton.AddToClassList("RecorderListHeader");
            recorderListPresetButton.AddToClassList("RecorderListHeader");
            
            addRecordButton.RegisterCallback<MouseUpEvent>(evt => ShowNewRecorderMenu());            
            
            m_AddNewRecordPanel = new VisualElement
            {
                name = "addRecordersButton",
                style = { flexDirection = FlexDirection.Row }
            };
            
                                    
            m_AddNewRecordPanel.Add(addRecordButton);
            m_AddNewRecordPanel.Add(recorderListPresetButton);

            m_RecordingListItem = new RecorderItemList
            {
                name = "recorderList",
                persistenceKey = "RecordersList",
                focusIndex = 0
            };

            UIElementHelper.SetFlex(m_RecordingListItem, 1.0f);
            
            m_RecordingListItem.OnItemContextMenu += OnRecordContextMenu;
            m_RecordingListItem.OnSelectionChanged += OnRecordSelectionChanged;
            m_RecordingListItem.OnItemRename += item => item.StartRenaming();
            m_RecordingListItem.OnContextMenu += ShowNewRecorderMenu;

            m_RecordersPanel.Add(m_AddNewRecordPanel);
            m_RecordersPanel.Add(m_RecordingListItem);

            m_ParametersControl = new VisualElement
            {
                style =
                {
                    minWidth = 300.0f,
                }
            };

            UIElementHelper.SetFlex(m_ParametersControl, 1.0f);
            
            m_ParametersControl.Add(new Button(OnRecorderSettingPresetClicked)
            {
                name = "presetButton",
                style =
                {
                    alignSelf = Align.FlexEnd,
                    backgroundImage = PresetHelper.presetIcon,
                    backgroundSize = ScaleMode.ScaleToFit,
                    sliceTop = 0,
                    sliceBottom = 0,
                    sliceLeft = 0,
                    sliceRight = 0,
                    paddingTop = 0.0f,
                    paddingLeft = 0.0f,
                    paddingBottom = 0.0f,
                    paddingRight = 0.0f,
                }
            });
            
            m_RecorderSettingPanel = new IMGUIContainer(OnRecorderSettingsGUI)
            {
                name = "recorderSettings"
            };

            UIElementHelper.SetFlex(m_RecorderSettingPanel, 1.0f);

            var statusBar = new VisualElement
            {
                name = "statusBar"
            };

            statusBar.Add(new IMGUIContainer(UpdateRecordingProgressGUI));
            
            root.Add(statusBar);
            
            m_ParametersControl.Add(m_RecorderSettingPanel);
            
            m_SettingsPanel.Add(m_ParametersControl);

            m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + s_PrefsFileName);
            m_RecorderController = new RecorderController(m_ControllerSettings);
            
            m_RecorderSettingsPrefsEditor = (RecorderSettingsPrefsEditor) Editor.CreateEditor(m_ControllerSettings);
            
            
#if UNITY_2018_2_OR_NEWER
            m_RecordingListItem.RegisterCallback<ValidateCommandEvent>(OnRecorderListValidateCommand);
            m_RecordingListItem.RegisterCallback<ExecuteCommandEvent>(OnRecorderListExecuteCommand);
#else
            m_RecordingListItem.RegisterCallback<IMGUIEvent>(OnRecorderListIMGUI);
#endif
            
            m_RecordingListItem.RegisterCallback<KeyUpEvent>(OnRecorderListKeyUp);
            
            ReloadRecordings();

            Undo.undoRedoPerformed += SaveAndRepaint;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Used to Start the recording with current settings.
        /// If not already the case, the Editor will also switch to PlayMode.
        /// </summary>
        public void StartRecording()
        {
            if (m_State == State.Idle)
            {
                m_State = State.WaitingForPlayModeToStartRecording;
                GameViewSize.DisableMaxOnPlay();
                EditorApplication.isPlaying = true;
                m_FrameCount = Time.frameCount;
            }
        }

        /// <summary>
        /// Used to get the current state of the recording.
        /// </summary>
        /// <returns>True if the Recorder is started or being started. False otherwise.</returns>
        public bool IsRecording()
        {
            return m_State == State.Recording || m_State == State.WaitingForPlayModeToStartRecording;
        }
        
        /// <summary>
        /// Used to Stop current recordings if any.
        /// Exiting PlayMode while the Recorder is recording will automatically Stop the recorder.
        /// </summary>
        public void StopRecording()
        {
            if (IsRecording())
                StopRecordingInternal();
        }

        void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                if (m_State == State.Recording)
                {
                    StopRecordingInternal();
                }
                else
                {
                    m_State = State.Idle;
                }
            }
        }

        void OnGlobalSettingsChanged()
        {
            if (m_ControllerSettings == null)
                return;
            
            m_ControllerSettings.ApplyGlobalSettingToAllRecorders();
            
            foreach (var recorderItem in m_RecordingListItem.items)
                recorderItem.UpdateState();

            SaveAndRepaint();
        }

        void SaveAndRepaint()
        {   
            if (m_ControllerSettings != null)
                m_ControllerSettings.Save();
            
            if (m_SelectedRecorderItem != null)
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            
            Repaint();
        }

        void ReloadRecordings()
        {
            if (m_ControllerSettings == null)
                return;

            m_ControllerSettings.ApplyGlobalSettingToAllRecorders();
            var recorderItems = m_ControllerSettings.recorderSettings.Select(CreateRecorderItem).ToArray();
            
            foreach (var recorderItem in recorderItems)
                recorderItem.UpdateState();
            
            m_RecordingListItem.Reload(recorderItems);
        }
        
#if UNITY_2018_2_OR_NEWER
        void OnRecorderListValidateCommand(ValidateCommandEvent evt)
        {
            RecorderListValidateCommand(evt, evt.commandName);
        }
        
        void OnRecorderListExecuteCommand(ExecuteCommandEvent evt)
        {
            RecorderListExecuteCommand(evt, evt.commandName); 
        }
#else
        void OnRecorderListIMGUI(IMGUIEvent evt)
        {           
            if (evt.imguiEvent.type == EventType.ValidateCommand)
            {
                RecorderListValidateCommand(evt, evt.imguiEvent.commandName);
            }
            else if (evt.imguiEvent.type == EventType.ExecuteCommand)
            {
                RecorderListExecuteCommand(evt, evt.imguiEvent.commandName);   
            }
        }
#endif

        void RecorderListValidateCommand(EventBase evt, string commandName)
        {
            if (m_RecordingListItem == null || m_RecordingListItem.selection == null)
                return;

            if (commandName == "Duplicate" || commandName == "SoftDelete" || commandName == "Delete")
            {
                evt.StopPropagation();
            }
        }
        
        void RecorderListExecuteCommand(EventBase evt, string commandName)
        {
            if (m_RecordingListItem == null)
                return;
            
            var item = m_RecordingListItem.selection;
            
            if (item == null)
                return;

            if (commandName == "Duplicate")
            {
                DuplicateRecorder(item);
                evt.StopPropagation();
            }
            else if (commandName == "SoftDelete" || commandName == "Delete")
            {
                DeleteRecorder(item, true);
                evt.StopPropagation();
            }
        }
        
        void OnRecorderListKeyUp(KeyUpEvent evt)
        {
            if (m_RecordingListItem == null)
                return;

            if (m_RecordingListItem.items == null || m_RecordingListItem.items.Count == 0)
                return;
            
            if (!m_RecordingListItem.HasFocus())
                return;
            
            if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
            {
                if (m_RecordingListItem.selection == null)
                {
                    m_RecordingListItem.selection = m_RecordingListItem.items.First();
                }
                else
                {
                    var currentIndex = m_RecordingListItem.selectedIndex;

                    var newIndex = ((evt.keyCode == KeyCode.UpArrow ? currentIndex - 1 : currentIndex + 1) + m_RecordingListItem.items.Count) %
                                   m_RecordingListItem.items.Count;

                    m_RecordingListItem.selectedIndex = newIndex;
                }
                
                evt.StopPropagation();
            }
        }
        
        void ApplyPreset(string presetPath)
        {           
            var candidate = AssetDatabase.LoadAssetAtPath<RecorderControllerSettingsPreset>(presetPath);

            if (candidate == null)
                return;
                
            candidate.AppyTo(m_ControllerSettings);
            ReloadRecordings();
        }

        void ShowNewRecorderMenu()
        {
            var newRecordMenu = new GenericMenu();
                
            foreach (var info in RecordersInventory.builtInRecorderInfos)
                AddRecorderInfoToMenu(info, newRecordMenu);

            if (Options.showLegacyRecorders)
            {
                newRecordMenu.AddSeparator(string.Empty);
                
                foreach (var info in RecordersInventory.legacyRecorderInfos)
                    AddRecorderInfoToMenu(info, newRecordMenu);
            }
                
            var recorderList = RecordersInventory.customRecorderInfos.ToList();

            if (recorderList.Any())
            {
                newRecordMenu.AddSeparator(string.Empty);
                
                foreach (var info in recorderList)
                    AddRecorderInfoToMenu(info, newRecordMenu);
            }
            
            newRecordMenu.ShowAsContext();
        }

        void AddRecorderInfoToMenu(RecorderInfo info, GenericMenu menu)
        {
            if (ShouldDisableRecordSettings())
                menu.AddDisabledItem(new GUIContent(info.displayName));
            else
                menu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
        }
        
        RecorderItem CreateRecorderItem(RecorderSettings recorderSettings)
        {
            var info = RecordersInventory.GetRecorderInfo(recorderSettings.GetType());
           
            var hasError = info == null; 
            
            var recorderItem = new RecorderItem(m_ControllerSettings, recorderSettings, hasError ? null : info.iconName);
            recorderItem.OnEnableStateChanged += enabled =>
            {
                if (enabled)
                    m_RecordingListItem.selection = recorderItem;
            };
            
            if (hasError)
                recorderItem.state = RecorderItem.State.HasErrors;

            return recorderItem;
        }

        string CheckRecordersIncompatibility()
        {
            var activeRecorders = m_ControllerSettings.recorderSettings.Where(r => r.enabled).ToArray();

            if (activeRecorders.Length == 0)
                return null;

            var outputPaths = new Dictionary<string, RecorderSettings>();

            foreach (var recorder in activeRecorders)
            {
                var path = recorder.fileNameGenerator.BuildAbsolutePath(null); // Does not detect all conflict or might have false positives
                if (outputPaths.ContainsKey(path))
                    return "Recorders '" + outputPaths[path].name + "' and '" + recorder.name + "' might try to save into the same output file.";

                outputPaths.Add(path, recorder);
            }

            var gameViewRecorders = new Dictionary<ImageHeight, RecorderSettings>();

            foreach (var recorder in activeRecorders)
            {
                var gameView = recorder.inputsSettings.FirstOrDefault(i => i is GameViewInputSettings) as GameViewInputSettings;
                if (gameView != null)
                {
                    if (gameViewRecorders.Any() && !gameViewRecorders.ContainsKey(gameView.outputImageHeight))
                    {
                        return "Recorders '" + gameViewRecorders.Values.First().name + "' and '" +
                               recorder.name +
                               "' are recording the Game View using different resolutions. This can lead to unexpected behaviour.";
                    }
                    
                    gameViewRecorders[gameView.outputImageHeight] = recorder;
                }
            }

            return null;
        }

        bool ShouldDisableRecordSettings()
        {
            return IsRecording();
        }

        void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (m_State == State.WaitingForPlayModeToStartRecording)
                {
                    StartRecordingInternal();
                }
            }
            else
            {
                if (m_State == State.Recording)
                {
                    StopRecordingInternal();
                }
            }
            
            var enable = !ShouldDisableRecordSettings();
                
            m_AddNewRecordPanel.SetEnabled(enable);
            m_ParametersControl.SetEnabled(enable && m_SelectedRecorderItem != null && m_SelectedRecorderItem.state != RecorderItem.State.HasErrors);
            m_RecordModeOptionsPanel.SetEnabled(enable);
            m_FrameRateOptionsPanel.SetEnabled(enable);

            if (HaveActiveRecordings())
            {
                if (IsRecording())
                {
                    SetRecordButtonsEnabled(EditorApplication.isPlaying && Time.frameCount - m_FrameCount > 5.0f);
                }
                else
                {
                    SetRecordButtonsEnabled(true);
                }
            }
            else
            {
                SetRecordButtonsEnabled(false);
            }

            UpdateRecordButtonText();

            if (m_State == State.Recording)
            {
                if (!m_RecorderController.IsRecording())
                    StopRecordingInternal();
                
                Repaint();
            }
        }

        void SetRecordButtonsEnabled(bool enabled)
        {
            m_RecordButton.SetEnabled(enabled);
            m_RecordButtonIcon.SetEnabled(enabled);
        }

        void StartRecordingInternal()
        {        
            if (Options.verboseMode)
                Debug.Log("Start Recording.");

            var success = m_RecorderController.StartRecording();
            
            if (success)
            {
                m_State = State.Recording;
                m_FrameCount = 0;
            }
            else
            {
                StopRecordingInternal();
                m_State = State.Error;
            }
        }

        void OnRecordButtonClick()
        {
            if (m_State == State.Error)
                m_State = State.Idle;
            
            switch (m_State)
            {
                case State.Idle:
                {          
                    StartRecording();
                    break;
                }

                case State.WaitingForPlayModeToStartRecording:
                case State.Recording:
                {
                    StopRecording();
                    break;
                }
            }
            
            UpdateRecordButtonText();
        }

        void UpdateRecordButtonText()
        {
            m_RecordButton.text = m_State == State.Recording ? "STOP RECORDING" : "START RECORDING";
        }
        
        void StopRecordingInternal()
        {
            if (Options.verboseMode)
                Debug.Log("Stop Recording.");
            
            m_RecorderController.StopRecording();
            
            m_State = State.Idle;
            m_FrameCount = 0;
            
            // Settings might have changed after the session ended
            m_ControllerSettings.Save();

            if (Options.exitPlayMode)
                EditorApplication.isPlaying = false;
        }
        
        static void PrepareGUIState(float contextWidth)
        {
            EditorGUIUtility.labelWidth = Mathf.Min(Mathf.Max(contextWidth * 0.45f - 40, 100), 160);
        }

        void OnRecorderSettingsGUI()
        {
            PrepareGUIState(m_RecorderSettingPanel.layout.width);
            
            if (m_SelectedRecorderItem != null)
            {               
                if (m_SelectedRecorderItem.state == RecorderItem.State.HasErrors)
                {
                    EditorGUILayout.LabelField("Missing reference to the recorder.");
                }
                else
                {
                    var editor = m_SelectedRecorderItem.editor;

                    if (editor == null)
                    {
                        EditorGUILayout.LabelField("Error while displaying the recorder inspector");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Recorder Type",
                            ObjectNames.NicifyVariableName(editor.target.GetType().Name));

                        if (!(editor is RecorderEditor))
                            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                        
                        EditorGUI.BeginChangeCheck();
                        
                        editor.OnInspectorGUI();

                        if (EditorGUI.EndChangeCheck())
                        {
                            m_ControllerSettings.Save();
                            m_SelectedRecorderItem.UpdateState();
                            m_RecorderSettingPanel.Dirty(ChangeType.Layout);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No recorder selected");
            }
        }

        void ShowRecorderListMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(Styles.SaveRecorderListLabel, false, () =>
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Preset", "RecorderSettingPreset.asset", "asset", "");

                if (path.Length != 0)
                    RecorderControllerSettingsPreset.SaveAtPath(m_ControllerSettings, path);
            });

            var presets = AssetDatabase.FindAssets("t:" + typeof(RecorderControllerSettingsPreset).Name);

            if (presets.Length > 0)
            {
                foreach (var preset in presets)
                {
                    var path = AssetDatabase.GUIDToAssetPath(preset);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    menu.AddItem(new GUIContent("Load Recorder List/" + fileName), false, data => { ApplyPreset((string)data); }, path);
                }
            }
            else
            {
                menu.AddDisabledItem(Styles.LoadRecorderListLabel);
            }

            var items = m_RecordingListItem.items.ToArray();

            if (items.Length > 0)
            {
                menu.AddItem(Styles.ClearRecorderListLabel, false, () =>
                {
                    if (EditorUtility.DisplayDialog("Clear Recoder List?", "All recorder will be deleted. Proceed?", "Delete Recorders", "Cancel"))
                    {
                        foreach (var item in items)
                        {
                            if (item.editor != null)
                                DestroyImmediate(item.editor);
                            
                            DeleteRecorder(item, false);
                        }
                        
                        ReloadRecordings();
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(Styles.ClearRecorderListLabel);
            }

            menu.ShowAsContext();
        }

        void OnRecorderSettingPresetClicked()
        {
            if (m_SelectedRecorderItem != null && m_SelectedRecorderItem.settings != null)
            {
                var presetReceiver = CreateInstance<PresetHelper.PresetReceiver>();
                presetReceiver.Init(m_SelectedRecorderItem.settings, Repaint, () => m_ControllerSettings.Save());
                
                PresetSelector.ShowSelector(m_SelectedRecorderItem.settings, null, true, presetReceiver);
            }
        }

        void OnDestroy()
        {
            if (IsRecording())
                StopRecording();
            
            if (m_ControllerSettings != null)
            {
                m_ControllerSettings.Save();
                DestroyImmediate(m_ControllerSettings);
            }

            if (m_RecorderSettingsPrefsEditor != null)
                DestroyImmediate(m_RecorderSettingsPrefsEditor);
            
            Undo.undoRedoPerformed -= SaveAndRepaint;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void AddLastAndSelect(RecorderSettings recorder, string desiredName, bool enabled)
        {
            recorder.name = GetUniqueRecorderName(desiredName);
            recorder.enabled = enabled;
            m_ControllerSettings.AddRecorderSettings(recorder);

            var item = CreateRecorderItem(recorder);
            m_RecordingListItem.Add(item);
            m_RecordingListItem.selection = item;
            m_RecordingListItem.Focus();
        }

        void DuplicateRecorder(RecorderItem item)
        {
            var candidate = item.settings;
            var copy = Instantiate(candidate);
            copy.OnAfterDuplicate();
            AddLastAndSelect(copy, candidate.name, candidate.enabled);
        }

        void DeleteRecorder(RecorderItem item, bool prompt)
        {
            if (!prompt || EditorUtility.DisplayDialog("Delete Recoder?",
                    "Are you sure you want to delete '" + item.settings.name + "' ?", "Delete", "Cancel"))
            {

                var s = item.settings;
                m_ControllerSettings.RemoveRecorder(s);
                UnityHelpers.Destroy(s, true);
                UnityHelpers.Destroy(item.editor, true);
                m_RecordingListItem.Remove(item);
            }
            
            if (prompt)
                Focus();
        }
        
        void OnAddNewRecorder(RecorderInfo info)
        {           
            var recorder = RecordersInventory.CreateDefaultRecorderSettings(info.settingsType);  
            AddLastAndSelect(recorder, ObjectNames.NicifyVariableName(info.displayName), true);
            
            m_RecorderSettingPanel.Dirty(ChangeType.All);
        }

        string GetUniqueRecorderName(string desiredName)
        {
            return ObjectNames.GetUniqueName(m_ControllerSettings.recorderSettings.Select(r => r.name).ToArray(),
                desiredName);
        }

        void OnRecordContextMenu(RecorderItem recorder)
        {
            var contextMenu = new GenericMenu();

            if (ShouldDisableRecordSettings())
            {
                contextMenu.AddDisabledItem(Styles.DuplicateLabel);
                contextMenu.AddDisabledItem(Styles.DeleteLabel);
            }
            else
            {
                contextMenu.AddItem(Styles.DuplicateLabel, false,
                    data =>
                    {
                        DuplicateRecorder((RecorderItem) data);
                    }, recorder);
                
                contextMenu.AddItem(Styles.DeleteLabel, false,
                    data =>
                    {
                        DeleteRecorder((RecorderItem) data, true);
                    }, recorder);
            }

            contextMenu.ShowAsContext();
        }
        
        void OnRecordSelectionChanged()
        {        
            m_SelectedRecorderItem = m_RecordingListItem.selection;
            
            foreach (var r in m_RecordingListItem.items)
            {
                r.SetItemSelected(m_SelectedRecorderItem == r);
            }

            if (m_SelectedRecorderItem != null)
            {
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            }

            Repaint();
        }

        bool HaveActiveRecordings()
        {
            return m_ControllerSettings.recorderSettings.Any(r => r.enabled);
        }

        static void ShowMessageInStatusBar(string msg, MessageType messageType)
        {
            var r = EditorGUILayout.GetControlRect();
            
            if (messageType != MessageType.None)
            {
                var iconR = r;
                iconR.width = iconR.height;

                var icon = messageType == MessageType.Error
                    ? StatusBarHelper.errorIcon
                    : (messageType == MessageType.Warning ? StatusBarHelper.warningIcon : StatusBarHelper.infoIcon);
                
                GUI.DrawTexture(iconR, icon);

                r.xMin = iconR.xMax + 5.0f;
            }
            
            var style = messageType == MessageType.Error
                ? StatusBarHelper.errorStyle
                : (messageType == MessageType.Warning ? StatusBarHelper.warningStyle : StatusBarHelper.infoStyle);

            GUI.Label(r, msg, style);
        }

        void UpdateRecordingProgressGUI()
        {
            if (m_State == State.Error)
            {
                if (!HaveActiveRecordings())
                {
                    ShowMessageInStatusBar("Unable to start recording because no recorder has been set.", MessageType.Warning);
                }
                else
                {
                    ShowMessageInStatusBar("Unable to start recording. Please check Console logs for details.", MessageType.Error);                    
                }
                
                return;
            }
            
            if (m_State == State.Idle)
            {                
                if (!HaveActiveRecordings())
                {
                    ShowMessageInStatusBar("No active recorder", MessageType.Info);
                }
                else
                {
                    var msg = CheckRecordersIncompatibility();
                    if (string.IsNullOrEmpty(msg))
                    {
                        ShowMessageInStatusBar("Ready to start recording", MessageType.None);
                    }
                    else
                    {
                        ShowMessageInStatusBar(msg, MessageType.Warning);
                    }
                }
                
                return;
            }

            if (m_State == State.WaitingForPlayModeToStartRecording)
            {
                EditorGUILayout.LabelField(Styles.WaitingForPlayModeLabel);
                return;
            }
            
            var recordingSessions = m_RecorderController.GetRecordingSessions();

            var session = recordingSessions.FirstOrDefault(); // Hack. We know each session uses the same global settings so take the first one...

            if (session == null)
                return;
                    
            var progressBarRect = EditorGUILayout.GetControlRect();
            
            var settings = session.settings;

            switch (settings.recordMode)
            {
                case RecordMode.Manual:
                {
                    var label = string.Format("{0} Frame(s) processed", session.frameIndex);
                    EditorGUI.ProgressBar(progressBarRect, 0, label);

                    break;
                }
                case RecordMode.SingleFrame:
                case RecordMode.FrameInterval:
                {
                    var label = session.frameIndex < settings.startFrame
                        ? string.Format("Skipping first {0} frame(s)...", settings.startFrame - 1)
                        : string.Format("{0} Frame(s) processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (session.frameIndex + 1) / (float) (settings.endFrame + 1), label);
                    break;
                }
                case RecordMode.TimeInterval:
                {
                    var label = session.currentFrameStartTS < settings.startTime
                        ? string.Format("Skipping first {0} second(s)...", settings.startTime)
                        : string.Format("{0} Frame(s) processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (float) session.currentFrameStartTS / (settings.endTime.Equals(0.0f) ? 0.0001f : settings.endTime), label);
                    
                    break;
                }
            }
        }
    }
}
