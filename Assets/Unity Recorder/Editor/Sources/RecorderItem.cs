using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    class RecorderItem : VisualElement
    {
        public RecorderSettings settings { get; private set; }
        public Editor editor { get; private set; }

        readonly EditableLabel m_EditableLabel;
        readonly Toggle m_Toggle;

        readonly Texture2D m_RecorderIcon;
        
        Texture2D m_Icon;

        public event Action<bool> OnEnableStateChanged;
        
        static readonly Dictionary<string, Texture2D> s_IconCache = new Dictionary<string, Texture2D>();

        bool m_Selected;
        bool m_Disabled;
        
        public void SetItemSelected(bool value)
        {
            m_Selected = value;
            if (value)
                AddToClassList("selected");
            else
                RemoveFromClassList("selected");
        }

        public void SetItemEnabled(RecorderControllerSettings prefs, bool value)
        {           
            m_Disabled = !value;
            settings.enabled = value;
            prefs.Save();
            
            m_EditableLabel.SetLabelEnabled(value);

            if (m_Toggle != null)
                UIElementHelper.SetToggleValue(m_Toggle, value);
            
            if (value)
                RemoveFromClassList("disabled");
            else
                AddToClassList("disabled");
            
            if (OnEnableStateChanged != null)
                OnEnableStateChanged.Invoke(value);
        }

        public enum State
        {
            None,
            Normal,
            HasWarnings,
            HasErrors
        }

        State m_State = State.None;

        public void UpdateState(bool checkForWarnings = true)
        {
            try
            {
                if (settings == null || settings.HasErrors())
                {
                    state = State.HasErrors;
                    return;
                }

                if (checkForWarnings && settings.HasWarnings())
                {
                    state = State.HasWarnings;
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception when getting recorder state: " + e);
            }

            state = State.Normal;
        }

        public State state
        {
            get { return m_State; }
            set
            {
                if (value == State.None)
                    return;
                
                if (m_State == value)
                    return;

                switch (m_State)
                {
                    case State.HasWarnings:
                        RemoveFromClassList("hasWarnings");
                        break;

                    case State.HasErrors:
                        RemoveFromClassList("hasErrors");
                        break;
                }

                switch (value)
                {
                    case State.HasWarnings:
                        AddToClassList("hasWarnings");
                        m_Icon = StatusBarHelper.warningIcon;
                        break;

                    case State.HasErrors:
                        AddToClassList("hasErrors");
                        m_Icon = StatusBarHelper.errorIcon;
                        break;

                    case State.Normal:
                        m_Icon = m_RecorderIcon;
                        break;
                }

                m_State = value;
            }
        }

        static Texture2D LoadIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
                return null;

            Texture2D icon;
            
            if (s_IconCache.TryGetValue(iconName, out icon))
                return icon;

            if (EditorGUIUtility.isProSkin)
                icon = Resources.Load<Texture2D>("d_" + iconName);   
            
            if (icon == null)
                icon = Resources.Load<Texture2D>(iconName);
            
            s_IconCache[iconName] = icon;
            
            return icon;
        }
        
        public RecorderItem(RecorderControllerSettings prefs, RecorderSettings recorderSettings, string iconName)
        {           
            settings = recorderSettings;

            if (settings != null)
                editor = Editor.CreateEditor(settings);

            UIElementHelper.SetFlex(this, 1.0f);
            style.flexDirection = FlexDirection.Row;

            m_Toggle = new Toggle(null);
            
            m_Toggle.OnToggle(() =>
            {

                SetItemEnabled(prefs, UIElementHelper.GetToggleValue(m_Toggle));
            });
            
            Add(m_Toggle);
                
            m_RecorderIcon = LoadIcon(iconName);
            
            if (m_RecorderIcon == null)
                m_RecorderIcon = LoadIcon("customrecorder_16");
       
            UpdateState(false);
            
            var iconContainer = new IMGUIContainer(() => // UIElement Image doesn't support tint yet. Use IMGUI instead.
            {   
                var r = EditorGUILayout.GetControlRect();
                r.width = r.height = Mathf.Max(r.width, r.height);
                
                var c = GUI.color;

                var newColor = Color.white;

                if (m_Disabled)
                {
                    newColor.a = 0.5f;
                }
                else
                {
                    if (!m_Selected)
                        newColor.a = 0.8f;    
                }

                GUI.color = newColor;
                
                GUI.DrawTexture(r, m_Icon);

                GUI.color = c;
            });
            
            iconContainer.AddToClassList("RecorderItemIcon");

            iconContainer.SetEnabled(false);
            
            Add(iconContainer);
            
            m_EditableLabel = new EditableLabel { text = settings.name };
            m_EditableLabel.OnValueChanged(newValue =>
            {
                settings.name = newValue;
                prefs.Save();
            });
            Add(m_EditableLabel);

            var recorderEnabled = settings.enabled;
            UIElementHelper.SetToggleValue(m_Toggle, recorderEnabled);

            SetItemEnabled(prefs, recorderEnabled);
        }
    
        public void StartRenaming()
        {
            m_EditableLabel.StartEditing();
        }
    }
}