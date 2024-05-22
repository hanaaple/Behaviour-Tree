using UnityEngine;

namespace BehaviourTreeGraph.Runtime.Node
{
    public class RootNode : BehaviourTreeGraphNode
    {
        [HideInInspector] public BehaviourTreeGraphNode child;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override NodeState OnUpdate()
        {
            return child.Update();
        }

        public override string GetStringData()
        {
            return "";
        }

        public override void LoadDataFromString(string loadData)
        {
        }

        public override BehaviourTreeGraphNode Clone()
        {
            RootNode node = Instantiate(this);
            node.Initialize();
            node.child = child.Clone();
            return node;
        }
    }
}