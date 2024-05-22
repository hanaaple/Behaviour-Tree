using BehaviourTreeGraph.Runtime.Attributes;

namespace BehaviourTreeGraph.Runtime.Node.Action
{
    [ActionNodeInfo("Action")]
    public abstract class ActionNode : BehaviourTreeGraphNode
    {
        public override string GetStringData()
        {
            return "";
        }

        public override void LoadDataFromString(string loadData)
        {
        }
    }
}