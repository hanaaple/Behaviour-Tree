using System;
using UnityEngine;

namespace BehaviourTreeGraph.Runtime
{
    // Inherit for Display Behaviour Tree Graph Editor
    public abstract class BehaviourTreeRunner : MonoBehaviour
    {
        public BehaviourTreeGraphAsset behaviourTree;

        public BlackBoard blackBoard;

        protected void Init()
        {
            behaviourTree = behaviourTree.Clone();
            behaviourTree.Bind(blackBoard);
        }
    }
}