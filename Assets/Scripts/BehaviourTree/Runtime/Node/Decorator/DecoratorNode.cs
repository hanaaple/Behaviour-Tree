using UnityEngine;

namespace BehaviourTreeGraph.Runtime.Node.Decorator
{
    public abstract class DecoratorNode : BehaviourTreeGraphNode
    {
        [HideInInspector] public BehaviourTreeGraphNode child;

        public override string GetStringData()
        {
            return "";
        }

        public override void LoadDataFromString(string loadData)
        {
        }

        public override BehaviourTreeGraphNode Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.Initialize();
            node.child = child.Clone();
            return node;
        }
    }
}