using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourTreeGraph.Runtime.Node.Composite
{
    public abstract class CompositeNode : BehaviourTreeGraphNode
    {
        [HideInInspector] public List<BehaviourTreeGraphNode> children = new();

        public override BehaviourTreeGraphNode Clone()
        {
            CompositeNode node = Instantiate(this);
            node.Initialize();
            node.children = children.ConvertAll(item => item.Clone());
            return node;
        }

        public void SortChildren()
        {
            children.Sort(SortByHorizontalPosition);
            EditorUtility.SetDirty(this);
        }

        private static int SortByHorizontalPosition(BehaviourTreeGraphNode left, BehaviourTreeGraphNode right)
        {
            return left.pos.xMin < right.pos.xMin ? -1 : 1;
        }
    }
}