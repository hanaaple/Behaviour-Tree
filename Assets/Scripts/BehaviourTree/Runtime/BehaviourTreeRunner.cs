using UnityEngine;

namespace BehaviourTreeGraph.Runtime
{
    // Inherit for Display Behaviour Tree Graph Editor
    public abstract class BehaviourTreeRunner : MonoBehaviour
    {
        public BehaviourTreeGraphAsset behaviourTree;

        protected void Init()
        {
            behaviourTree = behaviourTree.Clone();
        }
    }
}