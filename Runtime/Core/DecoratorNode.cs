using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public abstract class DecoratorNode : Node
    {
        [HideInInspector]
        public Node child;

        public override Node Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.name = name;
            if (child != null)
            {
                node.child = child.Clone();
                node.child.parent = node;
            }
            return node;
        }

        public override Node Duplicate()
        {
            DecoratorNode node = Instantiate(this);
            node.child = null;
            return node;
        }

        public override List<Node> SelectHeirarchy()
        {
            var res = child.SelectHeirarchy();
            res.Add(this);
            return res;
        }

        public override void OnDrawGizmos()
        {
            if (child != null && state == State.Running)
                child.OnDrawGizmos();
        }
    }
}