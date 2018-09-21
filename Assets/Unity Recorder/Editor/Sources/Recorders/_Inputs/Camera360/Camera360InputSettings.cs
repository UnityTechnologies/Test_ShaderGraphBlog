#if UNITY_2018_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("360 View")]
    [Serializable]
    public class Camera360InputSettings : ImageInputSettings
    {
        public ImageSource source = ImageSource.MainCamera;
        public string cameraTag;
        public bool flipFinalOutput = false;
        public bool renderStereo = true;
        public float stereoSeparation = 0.065f;
        public int mapSize = 1024;
        
        [SerializeField] int m_OutputWidth = 1024;
        [SerializeField] int m_OutputHeight = 2048;

        internal override Type inputType
        {
            get { return typeof(Camera360Input); }
        }

        public override int outputWidth
        {
            get { return m_OutputWidth; }
            set { m_OutputWidth = value; }
        }

        public override int outputHeight
        {
            get { return m_OutputHeight; }
            set { m_OutputHeight = value; }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (source == ImageSource.TaggedCamera && string.IsNullOrEmpty(cameraTag))
            {
                ok = false;
                errors.Add("Missing camera tag");
            }

            if (m_OutputWidth != (1 << (int)Math.Log(m_OutputWidth, 2)))
            {
                ok = false;
                errors.Add("Output width must be a power of 2.");
            }

            if (m_OutputWidth < 128 || m_OutputWidth > 8 * 1024)
            {
                ok = false;
                errors.Add( string.Format( "Output width must fall between {0} and {1}.", 128, 8*1024 ));
            }

            if (m_OutputHeight != (1 << (int)Math.Log(m_OutputHeight, 2)))
            {
                ok = false;
                errors.Add("Output height must be a power of 2.");
            }

            if (m_OutputHeight < 128 || m_OutputHeight > 8 * 1024)
            {
                ok = false;
                errors.Add( string.Format( "Output height must fall between {0} and {1}.", 128, 8*1024 ));
            }

            if (mapSize != (1 << (int)Math.Log(mapSize, 2)))
            {
                ok = false;
                errors.Add("Cube Map size must be a power of 2.");
            }

            if( mapSize < 16 || mapSize > 8 * 1024 )
            {
                ok = false;
                errors.Add( string.Format( "Cube Map size must fall between {0} and {1}.", 16, 8*1024 ));
            }

            if (renderStereo && stereoSeparation < float.Epsilon)
            {
                ok = false;
                errors.Add("Stereo separation value is too small.");
            }

            return ok;
        }
    }

}

#endif