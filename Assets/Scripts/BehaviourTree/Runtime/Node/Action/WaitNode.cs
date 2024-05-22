using UnityEngine;

namespace BehaviourTreeGraph.Runtime.Node.Action
{
    public class WaitNode : ActionNode
    {
        public float tick;

        private float m_Time;

        protected override void OnStart()
        {
            m_Time = Time.time;
        }

        protected override void OnStop()
        {
        }

        protected override NodeState OnUpdate()
        {
            if (Time.time - m_Time > tick)
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }
    }
}