using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeGraphEditor.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
        }

        private IMGUIContainer m_Container;

        private UnityEditor.Editor m_Editor;

        public void Initialize()
        {
            // Debug.LogWarning("InspectorView Constructor");
            m_Container = new IMGUIContainer();
            Add(m_Container);
        }

        public void UpdateSelection(BehaviourTreeEditorNode editorNode)
        {
            m_Container.Clear();
            if (m_Editor)
                UnityEngine.Object.DestroyImmediate(m_Editor);
            m_Container.onGUIHandler = default;
            if (editorNode == null)
            {
                return;
            }

            m_Editor = UnityEditor.Editor.CreateEditor(editorNode.node);
            m_Container.onGUIHandler += () =>
            {
                if (m_Editor == null || m_Editor.serializedObject == null ||
                    m_Editor.serializedObject.targetObject == null || m_Editor.target == null)
                {
                    return;
                }

                m_Editor.OnInspectorGUI();
            };
        }
    }
}