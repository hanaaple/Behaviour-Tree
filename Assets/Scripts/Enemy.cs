using BehaviourTreeGraph.Runtime;

public class Enemy : BehaviourTreeRunner
{
    void Start()
    {
        Init();
    }

    void Update()
    {
        if (behaviourTree.treeState is BehaviourTreeGraphNode.NodeState.Running or BehaviourTreeGraphNode.NodeState.Waiting)
        {
            behaviourTree.Update();
        }
    }
}
