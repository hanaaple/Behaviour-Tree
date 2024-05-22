namespace BehaviourTreeGraph.Runtime.Node.Decorator
{
    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override NodeState OnUpdate()
        {
            child.Update();
            return NodeState.Running;
        }
    }
}