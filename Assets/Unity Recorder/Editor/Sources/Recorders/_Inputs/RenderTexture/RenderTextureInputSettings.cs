using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Render Texture Asset")]
    [Serializable]
    public class RenderTextureInputSettings : ImageInputSettings
    {
        public RenderTexture renderTexture;
        public bool flipFinalOutput = false;
        
        internal override Type inputType
        {
            get { return typeof(RenderTextureInput); }
        }
        
        public override int outputWidth
        {
            get { return renderTexture == null ? 0 : renderTexture.width; }
            set
            {
                if (renderTexture != null)
                    renderTexture.width = value;
            }
        }

        public override int outputHeight
        {
            get { return renderTexture == null ? 0 : renderTexture.height; }
            set
            {
                if (renderTexture != null)
                    renderTexture.height = value;
            }
        }

        internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (renderTexture == null)
            {
                ok = false;
                errors.Add("Missing source render texture object/asset.");
            }

            return ok;
        }
    }
}
