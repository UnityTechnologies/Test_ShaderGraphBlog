using System;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Recorder
{
    class PanelSplitter : VisualElement
    {
        readonly VisualElement m_AffectedElement;

        bool m_Grabbed;
        Vector2 m_GrabbedMousePosition;

        float m_ElementOriginalWidth;

        const float k_SplitterWidth = 5.0f;
        
        [Serializable]
        class Width
        {
            public float value;
        }
        
        Width m_Width;
        
        void SetWidth(float value)
        {
            if (m_Width == null)
                return;
           
            m_Width.value = value;
            m_AffectedElement.style.width = value;

            SavePersistentData();
        }

        public PanelSplitter(VisualElement affectedElement)
        {
            m_AffectedElement = affectedElement;

            style.cursor = UIElementsEditorUtility.CreateDefaultCursorStyle(MouseCursor.ResizeHorizontal);
            style.width = k_SplitterWidth;
            style.minWidth = k_SplitterWidth;
            style.maxWidth = k_SplitterWidth;
            
            RegisterCallback<MouseDownEvent>(OnMouseDown, Capture.Capture);
            RegisterCallback<MouseMoveEvent>(OnMouseMove, Capture.Capture);
            RegisterCallback<MouseUpEvent>(OnMouseUp, Capture.Capture);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != (int) MouseButton.LeftMouse)
                return;
            
            if (m_Grabbed)
                return;

            this.TakeMouseCapture();

            m_Grabbed = true;
            m_GrabbedMousePosition = evt.mousePosition;
            m_ElementOriginalWidth = m_AffectedElement.style.width;
            
            evt.StopImmediatePropagation();
        }
        
        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_Grabbed)
                return;

            var delta = evt.mousePosition.x - m_GrabbedMousePosition.x;

            var newWidth = Mathf.Max(m_ElementOriginalWidth + delta, m_AffectedElement.style.minWidth);
          
            if (m_AffectedElement.style.maxWidth > 0.0f)
                newWidth = Mathf.Min(newWidth, m_AffectedElement.style.maxWidth);

            SetWidth(newWidth);
        }
        
        void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button != (int) MouseButton.LeftMouse)
                return;

            if (!m_Grabbed)
                return;

            m_Grabbed = false;
            this.ReleaseMouseCapture();
            
            evt.StopImmediatePropagation();
        }
        
        public override void OnPersistentDataReady()
        {
            base.OnPersistentDataReady();

            var key = GetFullHierarchicalPersistenceKey();

            m_Width = GetOrCreatePersistentData<Width>(m_Width, key);

            if (m_Width.value > 0.0f)
                m_AffectedElement.style.width = m_Width.value;
        }
    }
}