using System;


namespace UnityEditor.Recorder.FrameCapturer
{
    abstract class EncoderBase
    {
        protected EncoderBase()
        {
            AppDomain.CurrentDomain.DomainUnload += WaitAsyncDelete;
        }

        static void WaitAsyncDelete(object sender, EventArgs e)
        {
            fcAPI.fcWaitAsyncDelete();
        }

        public abstract void Release();
        public abstract bool IsValid();
    }

}
