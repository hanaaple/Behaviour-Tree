using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviourTreeGraph.Runtime;
using BehaviourTreeGraph.Runtime.Node;
using BehaviourTreeGraph.Runtime.Node.Composite;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeGraphEditor.Editor
{
    public class BehaviourTreeGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeGraphView, UxmlTraits>
        {
        }

        public Action<BehaviourTreeEditorNode> OnNodeSelected;

        private BehaviourTreeGraphAsset m_GraphAsset;
        private BehaviourTreeGraphViewSearchProvider m_SearchProvider;

        private readonly List<BehaviourTreeGraphNode> m_Nodes = new();

        public void Initialize(BehaviourTreeGraphEditorWindow window)
        {
            m_SearchProvider = ScriptableObject.CreateInstance<BehaviourTreeGraphViewSearchProvider>();
            m_SearchProvider.GraphView = this;
            m_SearchProvider.window = window;
            this.nodeCreationRequest = ShowSearchWindow;
        }

        public BehaviourTreeGraphView()
        {
            // Debug.Log("Constructor");

            var background = new GridBackground
            {
                name = "Grid"
            };
            Add(background);
            background.SendToBack();

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Builder/BehaviourTreeGraphViewStyle.uss");
            if (!styleSheet)
            {
                Debug.LogError("uss asset path Path가 잘못되었습니다.");
            }

            styleSheets.Add(styleSheet);

            var contentZoomer = new ContentZoomer();
            contentZoomer.maxScale = 1.5f;
            this.AddManipulator(contentZoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            Undo.undoRedoPerformed += UpdateGraphView;

            serializeGraphElements += CopyOperation;
            canPasteSerializedData += CanPaste;
            unserializeAndPaste += PasteOperation;
        }

        private void UpdateGraphView()
        {
            if (!m_GraphAsset)
            {
                return;
            }

            if (m_Nodes.Count > m_GraphAsset.nodes.Count)
            {
                var except = m_Nodes.Except(m_GraphAsset.nodes).ToArray();
                foreach (var node in except)
                {
                    AssetDatabase.RemoveObjectFromAsset(node);
                    m_Nodes.Remove(node);
                }
            }
            else if (m_Nodes.Count < m_GraphAsset.nodes.Count)
            {
                var except = m_GraphAsset.nodes.Except(m_Nodes).ToArray();
                foreach (var node in except)
                {
                    AssetDatabase.AddObjectToAsset(node, m_GraphAsset);
                    m_Nodes.Add(node);
                }
            }

            PopulateView(m_GraphAsset);

            // Asset, Node가 변했는지, 다른 Undo와 비교하여 알 수 없음
            AssetDatabase.SaveAssets();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            //Debug.LogWarning("OnViewChange");

            if (graphViewChange.movedElements != null)
            {
                foreach (var editorNode in graphViewChange.movedElements.OfType<BehaviourTreeEditorNode>())
                {
                    editorNode.SavePosition();
                }

                foreach (var node in m_GraphAsset.nodes.OfType<CompositeNode>())
                {
                    node.SortChildren();
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                Undo.RecordObject(m_GraphAsset, "Removed Elements");

                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is BehaviourTreeEditorNode editorNode)
                    {
                        m_GraphAsset.DeleteNode(editorNode.node);
                    }

                    if (element is Edge edge)
                    {
                        BehaviourTreeEditorNode parentNode = edge.output.node as BehaviourTreeEditorNode;
                        BehaviourTreeEditorNode childNode = edge.input.node as BehaviourTreeEditorNode;

                        BehaviourTreeGraphNode.RemoveChild(parentNode.node, childNode.node);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    BehaviourTreeEditorNode parentView = edge.output.node as BehaviourTreeEditorNode;
                    BehaviourTreeEditorNode childView = edge.input.node as BehaviourTreeEditorNode;
                    BehaviourTreeGraphNode.AddChild(parentView.node, childView.node);
                }
            }

            return graphViewChange;
        }

        private bool CanPaste(string data)
        {
            // Debug.Log("Can Paste?");
            var nodeDatas = data.Split("|||");
            foreach (var nodeData in nodeDatas)
            {
                var typeName = nodeData.Split("///")[0];
                var type = Type.GetType(typeName);
                if (type == null || !(type.IsSubclassOf(typeof(BehaviourTreeGraphNode)) || type == typeof(Edge)))
                {
                    return false;
                }
            }

            return true;
        }

        private void PasteOperation(string operationname, string data)
        {
            Debug.Log("Paste!");
            var splitedData = data.Split("|||");

            foreach (var stringData in splitedData)
            {
                if (string.IsNullOrEmpty(stringData)) continue;

                var splitData = stringData.Split("///");

                if (splitData.Length == 0) continue;

                PasteNode(splitData);
            }

            foreach (var stringData in splitedData)
            {
                if (string.IsNullOrEmpty(stringData)) continue;

                var splitData = stringData.Split("///");

                if (splitData.Length == 0) continue;

                PasteEdge(splitData);
            }
        }

        private string CopyOperation(IEnumerable<GraphElement> elements)
        {
            Debug.Log("Copy!");

            var editorNodes = elements.OfType<BehaviourTreeEditorNode>().ToArray();
            var edgeData = elements.OfType<Edge>();
            var stringBuilder = new StringBuilder();

            Dictionary<BehaviourTreeEditorNode, string> nodeGuid = new();

            // Copy Edge Data
            foreach (var edge in edgeData)
            {
                TryCopyEdge(stringBuilder, editorNodes, nodeGuid, edge);
            }

            // Copy Node Data
            foreach (var editorNode in editorNodes)
            {
                CopyNode(stringBuilder, nodeGuid, editorNode);
            }

            return stringBuilder.ToString();
        }

        private void PasteNode(IReadOnlyList<string> splitData)
        {
            var typeName = splitData[0];
            var type = Type.GetType(typeName);

            if (type == null || !type.IsSubclassOf(typeof(BehaviourTreeGraphNode))) return;

            var jsonData = splitData[1];

            var node = CreateNode(type, jsonData);

            var pos = new Rect(node.pos.position + new Vector2(10, 10), node.pos.size);
            var editorNode = GetNodeByGuid(node.viewDataKey);
            editorNode.SetPosition(pos);
            node.SetPosition(pos);
        }

        private void PasteEdge(IReadOnlyList<string> splitData)
        {
            var typeName = splitData[0];
            var type = Type.GetType(typeName);

            if (type == null || type != typeof(Edge)) return;

            var edgeData = splitData[1];
            var edgeGuid = edgeData.Split(",");
            var inputGuid = edgeGuid[0];
            var outputGuid = edgeGuid[1];
            var inputNode = GetNodeByGuid(inputGuid) as BehaviourTreeEditorNode;
            var outputNode = GetNodeByGuid(outputGuid) as BehaviourTreeEditorNode;

            // Debug.LogWarning($"Paste {outputGuid} -> {inputGuid}");

            BehaviourTreeGraphNode.AddChild(outputNode.node, inputNode.node);

            Edge edge = outputNode.output.ConnectTo(inputNode.input);
            AddElement(edge);
        }

        private static void TryCopyEdge(StringBuilder stringBuilder, BehaviourTreeEditorNode[] editorNodes,
            IDictionary<BehaviourTreeEditorNode, string> dictionary, Edge edge)
        {
            var inputNode = edge.input.node as BehaviourTreeEditorNode;
            var outputNode = edge.output.node as BehaviourTreeEditorNode;

            // if input & output is copied -> copy Edge
            if (editorNodes.Contains(inputNode) && editorNodes.Contains(outputNode))
            {
                if (!dictionary.TryGetValue(inputNode, out string inputGuid))
                {
                    inputGuid = Guid.NewGuid().ToString();
                    dictionary.Add(inputNode, inputGuid);
                }

                if (!dictionary.TryGetValue(outputNode, out string outputGuid))
                {
                    outputGuid = Guid.NewGuid().ToString();
                    dictionary.Add(outputNode, outputGuid);
                }

                // Debug.LogWarning($"Copy {outputGuid} -> {inputGuid}");

                stringBuilder.Append(edge.GetType().AssemblyQualifiedName);
                stringBuilder.Append("///");
                stringBuilder.Append(inputGuid);
                stringBuilder.Append(",");
                stringBuilder.Append(outputGuid);

                stringBuilder.Append("|||");
            }
        }

        private static void CopyNode(StringBuilder stringBuilder,
            IDictionary<BehaviourTreeEditorNode, string> dictionary, BehaviourTreeEditorNode editorNode)
        {
            if (!dictionary.TryGetValue(editorNode, out string guid))
            {
                guid = Guid.NewGuid().ToString();
            }

            var temp = editorNode.node.viewDataKey;
            
            stringBuilder.Append(editorNode.node.GetType().AssemblyQualifiedName);
            stringBuilder.Append("///");
            editorNode.node.SetGuid(guid);
            stringBuilder.Append(editorNode.node.GetJsonData());
            editorNode.node.SetGuid(temp);
            stringBuilder.Append("|||");
        }


        public void PopulateView(BehaviourTreeGraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_Nodes.Clear();
            // 기존의 View 제거
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);

            if (m_GraphAsset == null)
            {
                return;
            }

            graphViewChanged += OnGraphViewChanged;

            // Root Node 생성
            if (m_GraphAsset.rootNode == null)
            {
                var node = m_GraphAsset.CreateNode(typeof(RootNode)) as RootNode;
                m_GraphAsset.rootNode = node;

                AssetDatabase.SaveAssets();
            }

            // Debug.LogWarning("Draw");

            // Node 생성
            DrawNodes();

            // Edge 생성
            DrawConnections();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .Where(endPort => endPort != startPort && endPort.direction != startPort.direction &&
                                  endPort.node != startPort.node)
                .Where(endPort => endPort.portType == startPort.portType).ToList();
        }

        private void DrawNodes()
        {
            foreach (var node in m_GraphAsset.nodes)
            {
                CreateEditorNode(node);
            }
        }

        private void DrawConnections()
        {
            foreach (var parentNode in m_GraphAsset.nodes)
            {
                var children = BehaviourTreeGraphNode.GetChildren(parentNode);

                foreach (var child in children)
                {
                    DrawEditorConnection(parentNode, child);
                }
            }
        }

        private void DrawEditorConnection(BehaviourTreeGraphNode parent, BehaviourTreeGraphNode child)
        {
            var parentEditorNode = GetNodeByGuid(parent.viewDataKey) as BehaviourTreeEditorNode;
            var childEditorNode = GetNodeByGuid(child.viewDataKey) as BehaviourTreeEditorNode;

            if (parentEditorNode == null || childEditorNode == null)
            {
                Debug.LogWarning($"parent: {parentEditorNode}, child: {childEditorNode}  에러 발생");
                return;
            }

            Edge edge = parentEditorNode.output.ConnectTo(childEditorNode.input);
            AddElement(edge);
        }

        private void CreateEditorNode(BehaviourTreeGraphNode node)
        {
            m_Nodes.Add(node);
            var editorNode = new BehaviourTreeEditorNode(node);
            editorNode.SetPosition(node.pos);
            editorNode.OnNodeSelected = OnNodeSelected;
            AddElement(editorNode);
        }

        public void CreateNode(Type nodeType,
            Action<BehaviourTreeGraphNode> beforeDrawEditorAction = default, string guid = "")
        {
            Undo.RecordObject(m_GraphAsset, $"Created {nodeType.Name}");
            var node = m_GraphAsset.CreateNode(nodeType, guid);
            EditorUtility.SetDirty(m_GraphAsset);
            beforeDrawEditorAction?.Invoke(node);

            CreateEditorNode(node);
        }
        
        private BehaviourTreeGraphNode CreateNode(Type nodeType, string jsonData)
        {
            Undo.RecordObject(m_GraphAsset, $"Created {nodeType.Name}");
            var node = m_GraphAsset.CreateNode(nodeType);
            node.LoadDataFromJson(jsonData);
            node.InitializeState();
            EditorUtility.SetDirty(m_GraphAsset);

            CreateEditorNode(node);
            return node;
        }

        private void ShowSearchWindow(NodeCreationContext obj)
        {
            SearchWindow.Open(new SearchWindowContext(obj.screenMousePosition), m_SearchProvider);
        }

        public void UpdateNodeStates()
        {
            if (!Application.isPlaying)
                return;

            foreach (var editorNode in nodes.Select(node => node as BehaviourTreeEditorNode))
            {
                editorNode.UpdateState();
            }
        }

        public BehaviourTreeGraphAsset GetCurrentGraphAsset()
        {
            return m_GraphAsset;
        }
    }
}