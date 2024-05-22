using System;

namespace BehaviourTreeGraph.Runtime.Attributes
{
    public class ActionNodeInfoAttribute : Attribute
    {
        public string actionType { get; }

        public ActionNodeInfoAttribute(string actionType)
        {
            this.actionType = actionType;
        }
    }
}