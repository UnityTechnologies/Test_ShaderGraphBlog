using System;

namespace UnityEditor.Recorder
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    class RecorderSettingsAttribute : Attribute
    {
        public readonly Type recorderType;
        public readonly string displayName;
        public readonly string iconName;

        public RecorderSettingsAttribute(Type recorderType, string displayName)
        {
            this.recorderType = recorderType;
            this.displayName = displayName;
        }
        
        public RecorderSettingsAttribute(Type recorderType, string displayName, string iconName)
        {
            this.iconName = iconName;
            this.recorderType = recorderType;
            this.displayName = displayName;
        }
    }
}
