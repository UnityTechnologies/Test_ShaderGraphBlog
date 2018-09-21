using System;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    class ImageInputSelector : InputSettingsSelector
    {      
        [SerializeField] public GameViewInputSettings gameViewInputSettings = new GameViewInputSettings();
        [SerializeField] public CameraInputSettings cameraInputSettings = new CameraInputSettings();
        [SerializeField] public Camera360InputSettings camera360InputSettings = new Camera360InputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] public RenderTextureSamplerSettings renderTextureSamplerSettings = new RenderTextureSamplerSettings();
        
        public ImageInputSettings imageInputSettings
        {
            get { return (ImageInputSettings)selected; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value is CameraInputSettings ||
                    value is GameViewInputSettings ||
                    value is Camera360InputSettings ||
                    value is RenderTextureInputSettings ||
                    value is RenderTextureSamplerSettings)
                {
                    selected = value;
                }
                else
                {
                    throw new ArgumentException("Video input type not supported: '" + value.GetType() + "'");
                }
            }
        }

        public void ForceEvenResolution(bool value)
        {
            gameViewInputSettings.forceEvenSize = value;
            cameraInputSettings.forceEvenSize = value;
        }
    }
    
    [Serializable]
    class UTJImageInputSelector : InputSettingsSelector
    {      
        [SerializeField] public CameraInputSettings cameraInputSettings = new CameraInputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] public RenderTextureSamplerSettings renderTextureSamplerSettings = new RenderTextureSamplerSettings();
        
        public ImageInputSettings imageInputSettings
        {
            get { return (ImageInputSettings)selected; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value is CameraInputSettings ||
                    value is RenderTextureInputSettings ||
                    value is RenderTextureSamplerSettings)
                {
                    selected = value;
                }
                else
                {
                    throw new ArgumentException("Video input type not supported: '" + value.GetType() + "'");
                }
            }
        }
    }
}