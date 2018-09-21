#if UNITY_EDITOR

 namespace UnityEditor.Recorder.Examples
 {
     /// <exclude/>
     public static class RecorderEditorExample
     {
         [MenuItem(RecorderWindow.MenuRoot + "Examples/Start Recording", false, RecorderWindow.MenuRootIndex + 100)]
         static void StartRecording()
         {
             var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
             recorderWindow.StartRecording();
         }
    
         [MenuItem(RecorderWindow.MenuRoot + "Examples/Stop Recording", false, RecorderWindow.MenuRootIndex + 100)]
         static void StopRecording()
         {
             var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
             recorderWindow.StopRecording();
         }
     }
}

#endif
