namespace BehaviourTreeGraph.Runtime.Node.Composite
{
    /// <summary>
    /// Or Node
    /// </summary>
    public class SelectorNode : CompositeNode
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
                    case NodeState.Success:
                        return NodeState.Success;
                }
            }

            return NodeState.Failure;
        }
    }
}