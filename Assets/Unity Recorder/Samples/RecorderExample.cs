#if UNITY_EDITOR
 
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace UnityEngine.Recorder.Examples
{
    public class RecorderExample : MonoBehaviour
    {
       RecorderController m_RecorderController;
    
       void OnEnable()
       {
           var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
           m_RecorderController = new RecorderController(controllerSettings);

           var animationOutputFolder = Application.dataPath + "/SampleRecordings";
           var mediaOutputFolder = Application.dataPath + "../SampleRecordings";
           //var outputFolder = Application.dataPath + "/SampleRecordings";
    
           // Video
           var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
           videoRecorder.name = "My Video Recorder";
           videoRecorder.enabled = true;
    
           videoRecorder.outputFormat = VideoRecorderOutputFormat.MP4;
           videoRecorder.videoBitRateMode = VideoBitrateMode.Low;
    
           videoRecorder.imageInputSettings = new GameViewInputSettings
           {
               outputWidth = 1920,
               outputHeight = 1080
           };
    
           videoRecorder.audioInputSettings.preserveAudio = true;
    
           videoRecorder.outputFile = mediaOutputFolder + "/video_" + DefaultWildcard.Take;
    
           // Animation
           var animationRecorder = ScriptableObject.CreateInstance<AnimationRecorderSettings>();
           animationRecorder.name = "My Animation Recorder";
           animationRecorder.enabled = true;
    
           var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    
           animationRecorder.animationInputSettings = new AnimationInputSettings
           {
               gameObject = sphere, 
               recursive = true,
           };
           
           animationRecorder.animationInputSettings.AddComponentToRecord(typeof(Transform));
           
           animationRecorder.outputFile = animationOutputFolder + "/animation_" + DefaultWildcard.GeneratePattern("GameObject") + "_" + DefaultWildcard.Take;
    
           // Image Sequence
           var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
           imageRecorder.name = "My Image Recorder";
           imageRecorder.enabled = true;
    
           imageRecorder.outputFormat = ImageRecorderOutputFormat.PNG;
           imageRecorder.captureAlpha = true;
           
           imageRecorder.outputFile = mediaOutputFolder + "/image_" + DefaultWildcard.Frame + "_" + DefaultWildcard.Take;
    
           imageRecorder.imageInputSettings = new CameraInputSettings
           {
               source = ImageSource.MainCamera,
               outputWidth = 1920,
               outputHeight = 1080,
               captureUI = true
           };
    
           // Setup Recording
           controllerSettings.AddRecorderSettings(videoRecorder);
           controllerSettings.AddRecorderSettings(animationRecorder);
           controllerSettings.AddRecorderSettings(imageRecorder);
    
           controllerSettings.SetRecordModeToManual();
           controllerSettings.frameRate = 60.0f;
    
           Options.verboseMode = false;
           m_RecorderController.StartRecording();
       }
    
       void OnDisable()
       {
           m_RecorderController.StopRecording();
       }
    }
 }
    
 #endif
