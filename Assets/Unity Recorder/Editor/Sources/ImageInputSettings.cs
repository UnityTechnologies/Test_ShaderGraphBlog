using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    /// <inheritdoc />
    /// <summary>
    /// Optional base class for Image related inputs.
    /// </summary>
    public abstract class ImageInputSettings : RecorderInputSettings
    {       
        public abstract int outputWidth { get; set; }
        public abstract int outputHeight { get; set; }
        
        public virtual bool supportsTransparent
        {
            get { return true; }
        }
        
        public bool allowTransparency;
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Regroups settings needed to specify the size of an Image input using a size and an aspect ratio
    /// </summary>
    public abstract class StandardImageInputSettings : ImageInputSettings
    {
        [SerializeField] OutputResolution m_OutputResolution = new OutputResolution();
          
        internal bool forceEvenSize;
        
        public override int outputWidth
        {
            get { return ForceEvenIfNecessary(m_OutputResolution.GetWidth()); }
            set { m_OutputResolution.SetWidth(ForceEvenIfNecessary(value)); }
        }

        public override int outputHeight
        {
            get { return ForceEvenIfNecessary(m_OutputResolution.GetHeight()); }
            set { m_OutputResolution.SetHeight(ForceEvenIfNecessary(value)); }
        }
        
        internal ImageHeight outputImageHeight
        {
            get { return m_OutputResolution.imageHeight; }
            set { m_OutputResolution.imageHeight = value; }
        }
        
        internal ImageHeight maxSupportedSize
        {
            get { return m_OutputResolution.maxSupportedHeight; }
            set { m_OutputResolution.maxSupportedHeight = value; }
        }

        int ForceEvenIfNecessary(int v)
        {
            if (forceEvenSize && outputImageHeight != ImageHeight.Custom)
                return (v + 1) & ~1;

            return v;
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            var h = outputHeight;
            
            if (h > (int) maxSupportedSize)
            {
                ok = false;
                errors.Add("Output size exceeds maximum supported size: " + (int) maxSupportedSize );
            }

            var w = outputWidth;
            if (w <= 0 || h <= 0)
            {
                ok = false;
                errors.Add("Invalid output resolution: " + w + "x" + h);
            }
            
            return ok;
        }
    }
}