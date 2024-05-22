using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BehaviourTreeGraph.Runtime.Attributes;
using BehaviourTreeGraph.Runtime.Node.Action;
using BehaviourTreeGraph.Runtime.Node.Composite;
using BehaviourTreeGraph.Runtime.Node.Decorator;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeGraphEditor.Editor
{
    public struct SearchContextElement
    {
        public Type targetType { get; private set; }
        public string title { get; private set; }

        public SearchContextElement(Type targetType, string title)
        {
            this.targetType = targetType;
            this.title = title;
        }
    }

    public class BehaviourTreeGraphViewSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public BehaviourTreeGraphView GraphView;
        public EditorWindow window;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("Nodes")) };

            List<SearchContextElement> elements = new List<SearchContextElement>();

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types)
                {
                    elements.Add(new SearchContextElement(type, $"{type.BaseType?.Name}/{type.Name}"));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types)
                {
                    elements.Add(new SearchContextElement(type, $"{type.BaseType?.Name}/{type.Name}"));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    // 해당 ActionNode (Move)의 Type -> 기본 npc, 개, 몹, 엘리트 몹
                    ActionNodeInfoAttribute attribute =
                        type.GetCustomAttribute(typeof(ActionNodeInfoAttribute)) as ActionNodeInfoAttribute;
                    if (attribute != null)
                    {
                        elements.Add(new SearchContextElement(type,
                            $"{type.BaseType?.Name}/{attribute.actionType}/{type.Name}"));
                    }
                    else
                    {
                        elements.Add(new SearchContextElement(type,
                            $"{type.BaseType?.Name}/{type.Namespace}/{type.Name}"));
                    }
                }
            }

            // Sort By Name
            elements.Sort((entry1, entry2) =>
            {
                string[] splits1 = entry1.title.Split("/");
                string[] splits2 = entry2.title.Split("/");

                for (int i = 0; i < splits1.Length; i++)
                {
                    if (i >= splits2.Length)
                    {
                        return 1;
                    }

                    int value = string.Compare(splits1[i], splits2[i], StringComparison.Ordinal);

                    if (value != 0)
                    {
                        if (splits1.Length != splits2.Length && (i == splits1.Length || i == splits2.Length - 1))
                        {
                            return splits1.Length < splits2.Length ? 1 : -1;
                        }

                        return value;
                    }
                }

                return 0;
            });

            List<string> groups = new List<string>();

            // Add to search tree
            foreach (var element in elements)
            {
                string[] entryTitle = element.title.Split("/");

                StringBuilder groupNameBuilder = new StringBuilder();

                for (int i = 0; i < entryTitle.Length - 1; i++)
                {
                    groupNameBuilder.Append(entryTitle[i]);
                    var groupName = groupNameBuilder.ToString();

                    if (!groups.Contains(groupName))
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupNameBuilder.Append("/");
                }

                var entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()))
                {
                    level = entryTitle.Length,
                    userData = new SearchContextElement(element.targetType, element.title)
                };

                tree.Add(entry);
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition =
                GraphView.ChangeCoordinatesTo(GraphView, context.screenMousePosition - window.position.position);
            var graphMousePosition = GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            SearchContextElement element = (SearchContextElement)searchTreeEntry.userData;

            Type nodeType = element.targetType;

            GraphView.CreateNode(nodeType,
                node => { node.SetPosition(new Rect(graphMousePosition, new Vector2())); });

            return true;
        }
    }
}