using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using Halluvision.BehaviourTree;
using System.IO;

namespace Halluvision.BehaviourTree
{
    public class BehaviourTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected;
        public Action<bool> OnShiftKey;
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

        public BehaviourTreeEditor window;

        BehaviourTree tree;
        NodeSearchWindow searchWindow;
        List<NodeView> clipboard = new List<NodeView>();

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(BehaviourTreeEditor.AssetPath + "BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);
        }

        public void Init(BehaviourTreeEditor window)
        {
            this.window = window;
            AddSearchWindow();
            GenerateMiniMap();
            
            Undo.undoRedoPerformed += OnUndoRedo;
            RegisterCallback<DetachFromPanelEvent>(c => { Undo.undoRedoPerformed -= OnUndoRedo; });

            serializeGraphElements -= CutCopyAction;
            serializeGraphElements += CutCopyAction;
            canPasteSerializedData -= AllowPaste;
            canPasteSerializedData += AllowPaste;
            unserializeAndPaste -= OnPaste;
            unserializeAndPaste += OnPaste;

            RegisterCallback<KeyDownEvent>(KeyIsPressed);
            RegisterCallback<KeyUpEvent>(KeyIsReleased);
        }

        void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            var cords = this.contentViewContainer.WorldToLocal(new Vector2(window.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            Add(miniMap);
        }

        private void AddSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Init(window, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void OnUndoRedo()
        {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        void KeyIsPressed(KeyDownEvent _evt)
        {
            if (_evt.shiftKey)
                if (OnShiftKey != null)
                    OnShiftKey.Invoke(true);
        }

        void KeyIsReleased(KeyUpEvent _evt)
        {
            if (!_evt.shiftKey)
                if (OnShiftKey != null)
                    OnShiftKey.Invoke(false);
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public void PopulateView(BehaviourTree tree)
        {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            graphElements.ForEach(g => RemoveElement(g));
            graphViewChanged += OnGraphViewChanged;

            tree.nodes.ForEach(n =>
            {
                if (n is RootNode)
                {
                    tree.rootNode = n;
                }
            });

            if (tree.rootNode == null)
            {
                tree.rootNode = tree.CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
                tree.CreateBlackboard();
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }


            // Create NodeViews
            tree.nodes.ForEach(n => CreateNodeView(n));

            // Create Edges
            tree.nodes.ForEach(n =>
            {
                var children = tree.GetChildren(n);
                children.ForEach(c =>
                {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null)
                        tree.DeleteNode(nodeView.node);

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach(n =>
            {
                NodeView view = n as NodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        public Node CreateNode(System.Type type, Vector2 localPosition)
        {
            Node node = tree.CreateNode(type, localPosition);
            CreateNodeView(node);
            return node;
        }

        public Node DuplicateNode(Node _nodeToCopy, Vector2 _position)
        {
            Node node = tree.DuplicateNode(_nodeToCopy);
            node.position = _position;
            CreateNodeView(node);
            return node;
        }

        void CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        private string CutCopyAction(IEnumerable<GraphElement> elements)
        {
            var copiableElems = elements.Where(item =>
               {
                   var _nv = item as NodeView;
                   if (_nv != null)
                   {
                       if (_nv.node is RootNode)
                           return false;
                       else
                           return true;
                   }
                   else
                       return false;
               });

            clipboard = copiableElems.Cast<NodeView>().ToList();

            string _out = "";
            return _out;
        }

        bool AllowPaste(string data)
        {
            return true;
        }

        void OnPaste(string a, string b)
        {
           foreach(var _nv in clipboard)
            {
                DuplicateNode(_nv.node, _nv.node.position + new Vector2(150, 150));
                //var _assetPath = AssetDatabase.GetAssetPath(_nv.node);
                //string _newAssetPath = "";
                /*if (!AssetDatabase.CopyAsset(_assetPath, _newAssetPath))
                    Debug.LogError(_assetPath);*/

                //Node _assetNode = CreateNode(_nv.node.GetType(), _nv.node.position);
                //Node _newNode = _nv.node.Clone();
            }
        }

        public void UpdateNodeState()
        {
            nodes.ForEach(n =>
            {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }

        
    }
}