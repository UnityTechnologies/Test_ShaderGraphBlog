using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    /// <exclude/>
    public enum SuperSamplingCount
    {
        X1 = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8,
        X16 = 16,
    }

    [DisplayName("Texture Sampling")]
    [Serializable]
    public class RenderTextureSamplerSettings : ImageInputSettings
    {
        public ImageSource source = ImageSource.ActiveCamera;

        [SerializeField] int m_RenderWidth = 1280;
        [SerializeField] int m_RenderHeight = (int)ImageHeight.x720p_HD;


        [SerializeField] int m_OutputWidth = 1280;
        [SerializeField] int m_OutputHeight = (int)ImageHeight.x720p_HD;
        
        [SerializeField] internal AspectRatio outputAspectRatio = new AspectRatio();
        
        
        public SuperSamplingCount superSampling = SuperSamplingCount.X1;
        public float superKernelPower = 16f;
        public float superKernelScale = 1f;
        public string cameraTag;
        public ColorSpace colorSpace = ColorSpace.Gamma;
        public bool flipFinalOutput = false;

        internal readonly int kMaxSupportedSize = (int)ImageHeight.x2160p_4K;

        internal override Type inputType
        {
            get { return typeof(RenderTextureSampler); }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;
            
            var h = outputHeight;
            if (h > kMaxSupportedSize)
            {
                ok = false;
                errors.Add("Output size exceeds maximum supported size: " + kMaxSupportedSize);
            }
            
            return ok;
        }
        
        public override int outputWidth
        {
            get { return m_OutputWidth; }
            set { m_OutputWidth = Mathf.Min(16 * 1024, value); }
        }

        public override int outputHeight
        {
            get { return m_OutputHeight; }
            set { m_OutputHeight = value; }
        }

        public int renderWidth
        {
            get { return m_RenderWidth; }
            set { m_RenderWidth = value; }
        }
        
        public int renderHeight
        {
            get { return m_RenderHeight; }
            set { m_RenderHeight = value; }
        }
    }
}