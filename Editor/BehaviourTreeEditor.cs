using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using Halluvision.BehaviourTree;

namespace Halluvision.BehaviourTree
{
    public class BehaviourTreeEditor : EditorWindow
    {
        public static string AssetPath = "Assets/Plugins/BehaviourTree/Editor/";

        BehaviourTreeView treeView;
        InspectorView inspectorView;
        InspectorView blackboardView;

        bool isShiftPressed;

        [MenuItem("Halluvision/BehaviourTree Editor")]
        public static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.Show();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instancId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetPath + "BehaviourTreeEditor.uxml");
            visualTree.CloneTree(root);

            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetPath + "BehaviourTreeEditor.uss"));

            treeView = root.Q<BehaviourTreeView>();
            inspectorView = root.Q<InspectorView>("inspector-view");
            blackboardView = root.Q<InspectorView>("blackboard-inspector-view");

            treeView.Init(this);

            treeView.OnNodeSelected = OnNodeSelectionChanged;
            treeView.OnShiftKey = OnShiftKey;
            OnSelectionChange();
        }

        void OnShiftKey(bool _pressed)
        {
            isShiftPressed = _pressed;
        }

        private void OnSelectionChange()
        {
            BehaviourTree tree = Selection.activeObject as BehaviourTree;
            if (!tree)
            {
                if (Selection.activeGameObject)
                {
                    BehaviourTreeComponent component = Selection.activeGameObject.GetComponent<BehaviourTreeComponent>();
                    if (component)
                        tree = component.tree;
                }
            }

            if (Application.isPlaying)
            {
                if (tree)
                    treeView.PopulateView(tree);
            }
            else
            {
                if (tree && AssetDatabase.CanOpenForEdit(tree, StatusQueryOptions.UseCachedIfPossible))
                    treeView.PopulateView(tree);
            }

            if (tree != null)
            {
                SerializedObject treeObject = new SerializedObject(tree);
                blackboardView.UpdateSelection(tree.blackboard);
            }
        }

        void OnNodeSelectionChanged(NodeView node)
        {
            if (isShiftPressed)
            {
                var list = node.node.SelectHeirarchy();
                list.ForEach(n => treeView.AddToSelection(treeView.FindNodeView(n)));
            }

            inspectorView.UpdateSelection(node);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeState();
        }
    }
}