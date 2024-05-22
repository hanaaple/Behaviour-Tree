using System.Linq;
using BehaviourTreeGraph.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeGraphEditor.Editor
{
    public class BehaviourTreeGraphEditorWindow : EditorWindow
    {
        private BehaviourTreeGraphView m_GraphView;
        private InspectorView m_InspectorView;

        [MenuItem("BehaviourTree/Editor")]
        public static void OpenWindow()
        {
            var isAlreadyOpend = HasOpenInstances<BehaviourTreeGraphEditorWindow>();
            BehaviourTreeGraphEditorWindow window = GetWindow<BehaviourTreeGraphEditorWindow>();

            if (!isAlreadyOpend)
            {
                var iconTexture = EditorGUIUtility.Load("graph icon.png") as Texture;
                window.titleContent = new GUIContent($"BehaviourTree Editor", iconTexture);
            }

            window.OnSelectionChange();
        }

        // 더블 클릭 시 Window Open
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if (obj is BehaviourTreeGraphAsset && Selection.activeObject is BehaviourTreeGraphAsset)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        private void OnGUI()
        {
            var currentGraphAsset = m_GraphView?.GetCurrentGraphAsset();
            if (currentGraphAsset != null)
            {
                if (EditorUtility.IsDirty(currentGraphAsset) || currentGraphAsset.nodes.Any(EditorUtility.IsDirty))
                {
                    hasUnsavedChanges = true;
                }
                else
                {
                    hasUnsavedChanges = false;
                }
            }
        }

        public void CreateGUI()
        {
            // Debug.Log("CreateGUI");
            VisualElement root = rootVisualElement;

            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Builder/BehaviourTreeGraphEditor.uxml");
            if (!visualTree)
            {
                Debug.LogError("uxml asset path Path가 잘못되었습니다.");
            }

            visualTree.CloneTree(root);


            m_GraphView = root.Q<BehaviourTreeGraphView>();
            m_InspectorView = root.Q<InspectorView>();
            m_InspectorView.Initialize();

            m_GraphView.Initialize(this);

            m_GraphView.OnNodeSelected = OnNodeSelectionChanged;

            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            if (m_GraphView == null) return;

            BehaviourTreeGraphAsset graphAsset = Selection.activeObject as BehaviourTreeGraphAsset;

            // 컴포넌트 Select
            if (!graphAsset && Selection.activeGameObject)
            {
                if (Selection.activeGameObject.TryGetComponent(out BehaviourTreeRunner runner))
                {
                    graphAsset = runner.behaviourTree;
                }
            }

            // 런타임 View Update
            if (Application.isPlaying)
            {
                if (graphAsset)
                {
                    m_GraphView?.PopulateView(graphAsset);
                }
            }
            // 에디터 View Update
            else
            {
                if (graphAsset && AssetDatabase.CanOpenAssetInEditor(graphAsset.GetInstanceID()))
                {
                    m_GraphView?.PopulateView(graphAsset);
                }
            }
        }

        private void OnNodeSelectionChanged(BehaviourTreeEditorNode editorNode)
        {
            m_InspectorView.UpdateSelection(editorNode);
        }

        private void OnInspectorUpdate()
        {
            m_GraphView?.UpdateNodeStates();
        }

        private void OnPalyModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPalyModeStateChanged;
            EditorApplication.playModeStateChanged += OnPalyModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPalyModeStateChanged;
        }
    }
}