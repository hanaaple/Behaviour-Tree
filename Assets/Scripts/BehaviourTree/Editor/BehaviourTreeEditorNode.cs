using System;
using BehaviourTreeGraph.Runtime;
using BehaviourTreeGraph.Runtime.Node;
using BehaviourTreeGraph.Runtime.Node.Action;
using BehaviourTreeGraph.Runtime.Node.Composite;
using BehaviourTreeGraph.Runtime.Node.Decorator;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeGraphEditor.Editor
{
    public class BehaviourTreeEditorNode : Node
    {
        public Port input;
        public Port output;

        public Action<BehaviourTreeEditorNode> OnNodeSelected;

        public BehaviourTreeGraphNode node { get; private set; }

        public BehaviourTreeEditorNode(BehaviourTreeGraphNode node) : base("Assets/Ui Builder/EditorNode.uxml")
        {
            this.node = node;
            title = node.name;
            viewDataKey = node.viewDataKey;

            // Debug.LogWarning($"{title}, {viewDataKey}");

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();

            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses()
        {
            switch (node)
            {
                case ActionNode:
                    AddToClassList("action");
                    break;
                case CompositeNode:
                    AddToClassList("composite");
                    break;
                case DecoratorNode:
                    AddToClassList("decorator");
                    break;
                case RootNode:
                    AddToClassList("root");
                    break;
            }
        }

        private void CreateInputPorts()
        {
            if (node is ActionNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            }
            else if (node is CompositeNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            }
            else if (node is DecoratorNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            }

            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if (node is CompositeNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }
            else if (node is DecoratorNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }
            else if (node is RootNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }

            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            switch (node.state)
            {
                case BehaviourTreeGraphNode.NodeState.Running:
                    if (node.isStarted)
                        AddToClassList("running");
                    break;
                case BehaviourTreeGraphNode.NodeState.Failure:
                    AddToClassList("failure");
                    break;
                case BehaviourTreeGraphNode.NodeState.Success:
                    AddToClassList("success");
                    break;
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public void SavePosition()
        {
            Undo.RecordObject(node, "Moved Node");
            node.SetPosition(GetPosition());
            EditorUtility.SetDirty(node);
        }
    }
}