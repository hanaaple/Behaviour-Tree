namespace BehaviourTreeGraph.Runtime.Node.Composite
{
    /// <summary>
    /// And Node
    /// </summary>
    public class SequeneceNode : CompositeNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override NodeState OnUpdate()
        {
            if (children == null || children.Count == 0)
                return NodeState.Failure;

            foreach (var child in children)
            {
                child.SetState(NodeState.Waiting);
            }

            foreach (var child in children)
            {
                switch (child.Update())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        return NodeState.Failure;
                }
            }

            return NodeState.Success;
        }
    }
}