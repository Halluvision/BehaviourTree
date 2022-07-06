using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    [CreateAssetMenu(menuName = "Halluvision/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject
    {
        [HideInInspector]
        public Node rootNode;
        [HideInInspector]
        public Node.State treeState = Node.State.Running;
        [HideInInspector]
        public List<Node> nodes = new List<Node>();
        //[HideInInspector]
        public Blackboard blackboard;

        ActionNode activeNode;
        
        private void OnEnable()
        {
        }

        public Node.State Update()
        {
            if (rootNode.state == Node.State.Running)
                treeState = rootNode.Update();

            return treeState;
        }

        public void SetAsActiveNode(ActionNode node)
        {
            activeNode = node;
        }

        public void AbortActive(AbortType abortType, DecoratorNode node)
        {
            switch (abortType)
            {
                case AbortType.None:
                    break;
                case AbortType.Self:
                    if(IsInSameHierarchy(node, activeNode))
                        activeNode.Abort();
                    break;
                case AbortType.LowerPriority:
                    if (IsInLowerSibling(node, activeNode))
                        activeNode.Abort();
                    break;
                case AbortType.Both:
                    if (IsInSameHierarchy(node, activeNode))
                        activeNode.Abort();
                    else if (IsInLowerSibling(node, activeNode))
                        activeNode.Abort();
                    break;
            }
        }

        public void OnDrawGizmos()
        {
            if (rootNode.state == Node.State.Running)
                rootNode.OnDrawGizmos();
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type, Vector2 position)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.position = position;

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(node, this);

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public Node DuplicateNode(Node _nodeToCopy)
        {
            Node node = _nodeToCopy.Duplicate();
            node.guid = GUID.Generate().ToString();
            node.name = _nodeToCopy.name;

            Undo.RecordObject(this, "Behaviour Tree (DuplicateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(node, this);

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (DuplicateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            RootNode root = parent as RootNode;
            if (root)
            {
                Undo.RecordObject(root, "Behaviour Tree (AddChild)");
                root.child = child;
                child.parent = root;
                EditorUtility.SetDirty(root);
            }


            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                decorator.child = child;
                child.parent = decorator;
                EditorUtility.SetDirty(decorator);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                child.parent = composite;
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child)
        {
            RootNode root = parent as RootNode;
            if (root)
            {
                Undo.RecordObject(root, "Behaviour Tree (RemoveChild)");
                root.child = null;
                EditorUtility.SetDirty(root);
            }


            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void CreateBlackboard()
        {
            Blackboard bb = ScriptableObject.CreateInstance(typeof(Blackboard)) as Blackboard;
            bb.name = "Blackboard";
            
            blackboard = bb;

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(bb, this);

            AssetDatabase.SaveAssets();
        }
#endif

        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            RootNode root = parent as RootNode;
            if (root && root.child != null)
                children.Add(root.child);

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator && decorator.child != null)
                children.Add(decorator.child);

            CompositeNode composite = parent as CompositeNode;
            if (composite)
                return composite.children;

            return children;
        }

        List<Node> GetLowerSiblings(Node node)
        {
            List<Node> temp = new List<Node>();
            int index = GetChildren(node.parent).IndexOf(node);
            for (int i = index + 1; i < GetChildren(node.parent).Count; i++)
            {
                temp.Add(GetChildren(node.parent)[i]);
            }
            return temp;
        }

        private void Traverse(Node node, System.Action<Node> visitor)
        {
            if (node)
            {
                visitor.Invoke(node);
                var children = GetChildren(node);
                children.ForEach(n => Traverse(n, visitor));
            }
        }

        bool IsInSameHierarchy(Node parent, Node child)
        {
            if (child == null || parent == null)
                return false;

            Node temp = child.parent;
            while (temp != null)
            {
                if (System.Object.ReferenceEquals(temp, parent))
                    return true;
                temp = temp.parent;
            }
            return false;
        }

        bool IsInLowerSibling(Node node, Node child)
        {
            if (child == null || node == null)
                return false;

            foreach (var n in GetLowerSiblings(node))
            {
                if (IsInSameHierarchy(n, child))
                    return true;
            }
            return false;
        }

        public BehaviourTree Clone()
        {
            BehaviourTree _tree = Instantiate(this);
            _tree.blackboard = Instantiate(blackboard);
            _tree.rootNode = _tree.rootNode.Clone();
            _tree.nodes = new List<Node>();
            Traverse(_tree.rootNode, n => { _tree.nodes.Add(n); });
            Traverse(_tree.rootNode, n => { n.blackboard = _tree.blackboard; });
            return _tree;
        }

        public void Bind(BehaviourTreeComponent btComponent)
        {
            Traverse(rootNode, node =>
            {
                node.behaviourTreeComponent = btComponent;
                node.blackboard = blackboard;
                node.tree = this;
            });
        }
    }
}