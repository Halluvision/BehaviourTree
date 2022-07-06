using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public abstract class BehaviourTreeComponent : MonoBehaviour
    {
        public BehaviourTree tree;
        [SerializeField]
        bool pauseBehaviorTree = false;

        void Start()
        {
            if (tree != null)
            {
                tree = tree.Clone();
                tree.Bind(this);
            }
            else
            {
                Debug.LogError("behavior Tree is empty.", this);
            }
        }

        void Update()
        {
            if (!pauseBehaviorTree)
            {
                if (tree != null)
                    tree.Update();
            }
        }

        public void PauseBehaviorTree()
        {
            pauseBehaviorTree = true;
        }

        public void ResumeBehaviourTree()
        {
            pauseBehaviorTree = false;
        }

        private void OnDrawGizmos()
        {
            if (tree != null)
                tree.OnDrawGizmos();
        }
    }
}