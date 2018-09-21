using System;
using System.IO;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(FileNameGenerator))]
    class FileNameGeneratorDrawer : TargetedPropertyDrawer<FileNameGenerator>
    {
        SerializedProperty m_FileName;
        SerializedProperty m_Path;

        static bool s_Dirty = false;
        
        static readonly GUIStyle s_PathPreviewStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
	    static readonly GUIStyle s_OpenPathButtonStyle = new GUIStyle("minibutton") { fixedWidth = 30 };

	    static Texture2D s_OpenPathIcon;

        protected override void Initialize(SerializedProperty property)
        {
	        if (s_OpenPathIcon == null)
	        {
		        var iconName = "popout_icon";
		        if (EditorGUIUtility.isProSkin)
			        iconName = "d_" + iconName;
		        
		        s_OpenPathIcon = Resources.Load<Texture2D>(iconName);
	        }
	        
            if (target != null)
                return;
            
            base.Initialize(property);
            
            m_FileName = property.FindPropertyRelative("m_FileName");
            m_Path = property.FindPropertyRelative("m_Path");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float tagWidth = 77;
            var txtWidth = position.width - tagWidth - 5;
            var txtRect = new Rect(position.x, position.y, txtWidth, position.height);
            var tagRect = new Rect(position.x + txtWidth + 5, position.y, tagWidth, position.height);
            
            GUI.SetNextControlName("FileNameField");
            m_FileName.stringValue = GUI.TextField(txtRect, m_FileName.stringValue);
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            
            if (GUI.GetNameOfFocusedControl().Equals("FileNameField") && 
                Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
            {
                if (Event.current.keyCode == KeyCode.C)
                {
                    Event.current.Use();
                    editor.Copy();
                }
                else if (Event.current.keyCode == KeyCode.V)
                {
                    Event.current.Use();
                    editor.Paste();
                    m_FileName.stringValue = editor.text;
                }
            }

            if (EditorGUI.DropdownButton(tagRect, new GUIContent("+ Wildcards"), FocusType.Passive))
            {
                var menu = new GenericMenu();

                foreach (var w in target.wildcards)
                {
                    var pattern = w.pattern;
                    menu.AddItem(new GUIContent(w.label), false, () =>
                    {
                        m_FileName.stringValue = InsertTag(pattern, m_FileName.stringValue, editor);
                        m_FileName.serializedObject.ApplyModifiedProperties();
                        s_Dirty = true;
                    });
                }
                
                menu.DropDown(tagRect);
            }

            if (s_Dirty)
            {
                s_Dirty = false;
                GUI.changed = true;
            }

            EditorGUILayout.PropertyField(m_Path);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
	        
	        var path = target.BuildAbsolutePath(null);
	        
	        var r = GUILayoutUtility.GetRect(new GUIContent(path), s_PathPreviewStyle, null);
	        EditorGUI.SelectableLabel(r, path, s_PathPreviewStyle);
	        	        
	        if (GUILayout.Button(s_OpenPathIcon, s_OpenPathButtonStyle))
				OpenInFileBrowser.Open(path);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.EndProperty();
        }

        static string InsertTag(string pattern, string text, TextEditor editor)
        {
            if (!string.IsNullOrEmpty(editor.text)) // HACK If editor is not focused on
            {
                try
                {
                    editor.ReplaceSelection(pattern);
                    return editor.text;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return text + pattern;
        }
    }
    
	// Inspired from http://wiki.unity3d.com/index.php/OpenInFileBrowser
    static class OpenInFileBrowser
	{
		static void OpenInOSX(string path, bool openInsideFolder)
		{
			var osxPath = path.Replace("\\", "/");
	 
			if (!osxPath.StartsWith("\""))
			{
				osxPath = "\"" + osxPath;
			}
	 
			if (!osxPath.EndsWith("\""))
			{
				osxPath = osxPath + "\"";
			}
	 
			var arguments = (openInsideFolder ? "" : "-R ") + osxPath;
	 
			try
			{
				System.Diagnostics.Process.Start("open", arguments);
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				// tried to open mac finder in windows
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}
	
		static void OpenInWindows(string path, bool openInsideFolder)
		{ 
			var winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
	 
			try
			{
				System.Diagnostics.Process.Start("explorer.exe", (openInsideFolder ? "/root," : "/select,") + winPath);
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				// tried to open win explorer in mac
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}
	 
		public static void Open(string path)
		{
			if (!File.Exists(path))
				path = Path.GetDirectoryName(path);

			var openInsideFolder = Directory.Exists(path);
			
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				OpenInWindows(path, openInsideFolder);
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				OpenInOSX(path, openInsideFolder);
			}
		}
}

}