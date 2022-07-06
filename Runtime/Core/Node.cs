using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public abstract class Node : ScriptableObject
    {
        public enum State
        {
            Running,
            Failure,
            Success
        }

        [HideInInspector]
        public BehaviourTree tree;
        [HideInInspector]
        public Node parent;
        [HideInInspector]
        public State state = State.Running;
        [HideInInspector]
        public bool started = false;
        [HideInInspector]
        public string guid;
        [HideInInspector]
        public Vector2 position;
        [HideInInspector]
        public Blackboard blackboard;
        [HideInInspector]
        public BehaviourTreeComponent behaviourTreeComponent;
        [TextArea]
        public string description;

        public State Update()
        {
            if (!started)
            {
                if (this is ActionNode)
                    tree.SetAsActiveNode(this as ActionNode);
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state == State.Failure || state == State.Success)
            {
                started = false;
            }

            return state;
        }

        public virtual Node Clone()
        {
            Node node = Instantiate(this);
            node.name = name;
            return node;
        }

        public virtual Node Duplicate()
        {
            return Instantiate(this);
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
        public void Abort() { started = false; OnStop(); }
        public virtual List<Node> SelectHeirarchy() 
        {
            var n = new List<Node>();
            n.Add(this);
            return n; 
        }
        public virtual void OnDrawGizmos() { }
    }
}