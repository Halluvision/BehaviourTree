using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Halluvision.BehaviourTree
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        BehaviourTreeView treeView;
        BehaviourTreeEditor window;
        Texture2D identationIcon;

        public void Init(BehaviourTreeEditor window, BehaviourTreeView treeView)
        {
            this.window = window;
            this.treeView = treeView;

            // hack for indentation
            identationIcon = new Texture2D(1, 1);
            identationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            identationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return FillList();
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
            var localMousePosition = treeView.contentViewContainer.WorldToLocal(worldMousePosition);
            var userObject = SearchTreeEntry.userData as UserObject;
            treeView.CreateNode(userObject.type, localMousePosition);
            DestroyImmediate(userObject);
            return true;
        }

        List<SearchTreeEntry> FillList()
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("create Nodes"), 0));

            {
                searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Decorators"), 1));

                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract)
                        searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(type.Name, identationIcon)) { userData = new UserObject(type), level = 2 });
                }
            }

            {
                searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Composite"), 1));

                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract)
                        searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(type.Name, identationIcon)) { userData = new UserObject(type), level = 2 });
                }
            }

            {
                searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Actions"), 1));

                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract)
                        searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(type.Name, identationIcon)) { userData = new UserObject(type), level = 2 });
                }
            }

            return searchTreeEntries;
        }

        class UserObject : UnityEngine.Object
        {
            public Type type;
            public UserObject(Type type)
            {
                this.type = type;
            }
        }
    }
}