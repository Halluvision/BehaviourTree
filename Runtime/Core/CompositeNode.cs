using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public abstract class CompositeNode : Node
    {
        [HideInInspector]
        public List<Node> children = new List<Node>();

        public override Node Clone()
        {
            CompositeNode node = Instantiate(this);
            node.name = name;
            node.children = children.ConvertAll(c => c.Clone());
            node.children.ForEach(c => c.parent = node);
            return node;
        }

        public override Node Duplicate()
        {
            CompositeNode node = Instantiate(this);
            node.children = new List<Node>();//children.ConvertAll(c => c.Clone());
            return node;
        }

        public override List<Node> SelectHeirarchy()
        {
            List<Node> res = new List<Node>();
            children.ForEach(child => res.AddRange(child.SelectHeirarchy()));
            res.Add(this);
            return res;
        }

        public override void OnDrawGizmos()
        {
            if (state == State.Running)
                children.ForEach(c => c.OnDrawGizmos());
        }
    }
}