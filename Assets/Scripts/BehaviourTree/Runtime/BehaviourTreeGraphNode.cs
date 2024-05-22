using System;
using System.Collections.Generic;
using BehaviourTreeGraph.Runtime.Node;
using BehaviourTreeGraph.Runtime.Node.Composite;
using BehaviourTreeGraph.Runtime.Node.Decorator;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourTreeGraph.Runtime
{
    [Serializable]
    public abstract class BehaviourTreeGraphNode : ScriptableObject
    {
        public enum NodeState
        {
            Waiting,
            Running,
            Failure,
            Success
        }

        // Bind to node Label "descript" 
        [SerializeField] [TextArea] private string description;

        // If you want to update Position on moving, SetPosition() is not work immediately -> GetPosition() returns wrong data.
        [SerializeField] [HideInInspector] private Rect position;
        [SerializeField] [HideInInspector] private string guid;
        [SerializeField] [HideInInspector] private NodeState nodeState = NodeState.Waiting;
        [SerializeField] [HideInInspector] private bool started;

        public UnityAction<NodeState> OnChangeState = null;

        public string viewDataKey => guid;
        public Rect pos => position;
        public NodeState state => nodeState;
        public bool isStarted => started;

        public void Initialize(string nodeName = "", string id = "")
        {
            if (string.IsNullOrEmpty(nodeName))
                nodeName = GetType().Name;
            name = nodeName;

            //Debug.LogWarning($"{id},  {string.IsNullOrEmpty(id)}");
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid().ToString();
            }
            else
            {
                guid = id;
            }

            // Debug.LogWarning($"{name},  {m_Position}   {m_Guid}");
        }

        public NodeState Update()
        {
            nodeState = NodeState.Running;
            if (!started)
            {
                OnStart();
                started = true;
            }

            nodeState = OnUpdate();

            if (nodeState is NodeState.Failure or NodeState.Success)
            {
                OnStop();
                started = false;
            }

            return nodeState;
        }

        public void SetPosition(Rect pos)
        {
            // Debug.LogWarning($"Set Position {m_Position.position} -> {pos.position}");
            position = pos;
        }

        public virtual BehaviourTreeGraphNode Clone()
        {
            // Debug.LogWarning($"Original:  {name},  {m_Position}   {m_Guid}");
            var node = Instantiate(this);
            node.Initialize();
            return node;
        }

        private void OnValidate()
        {
            OnChangeState?.Invoke(state);
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract NodeState OnUpdate();

        /// <summary>
        /// Convert Node Data to String
        /// Try Implement for Copy field data
        /// 
        /// Do Not use Separator "///", "|||"
        /// </summary>
        /// <returns> return String Data </returns>
        public abstract string GetStringData();

        /// <summary>
        /// Try Implement for Paste field data
        /// </summary>
        public abstract void LoadDataFromString(string loadData);

        public static void AddChild(BehaviourTreeGraphNode parent, BehaviourTreeGraphNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(parent, "Add Connection");
#endif

            if (parent is DecoratorNode decoratorNode)
            {
                decoratorNode.child = child;
            }
            else if (parent is RootNode rootNode)
            {
                rootNode.child = child;
            }
            else if (parent is CompositeNode compositeNode)
            {
                compositeNode.children.Add(child);
            }
        }

        public static void RemoveChild(BehaviourTreeGraphNode parent, BehaviourTreeGraphNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(parent, "Removed Connection");
#endif
            if (parent is DecoratorNode decoratorNode)
            {
                decoratorNode.child = null;
            }
            else if (parent is RootNode rootNode)
            {
                rootNode.child = null;
            }
            else if (parent is CompositeNode compositeNode)
            {
                compositeNode.children.Remove(child);
            }
        }

        public static List<BehaviourTreeGraphNode> GetChildren(BehaviourTreeGraphNode parent)
        {
            var children = new List<BehaviourTreeGraphNode>();
            if (parent is DecoratorNode decoratorNode && decoratorNode.child != null)
            {
                children.Add(decoratorNode.child);
            }
            else if (parent is RootNode rootNode && rootNode.child != null)
            {
                children.Add(rootNode.child);
            }
            else if (parent is CompositeNode compositeNode)
            {
                children.AddRange(compositeNode.children);
            }

            return children;
        }

        public void SetState(NodeState nodeState)
        {
            this.nodeState = nodeState;
        }
    }
}