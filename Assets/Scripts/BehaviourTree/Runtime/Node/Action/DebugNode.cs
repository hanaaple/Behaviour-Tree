using UnityEngine;

namespace BehaviourTreeGraph.Runtime.Node.Action
{
    public class DebugNode : ActionNode
    {
        public string message;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override NodeState OnUpdate()
        {
            Debug.Log($"Message: {message}");
            return NodeState.Success;
        }
    }
}