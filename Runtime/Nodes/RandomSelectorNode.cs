using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public class RandomSelectorNode : CompositeNode
    {
        protected override void OnStart()
        {
            ShuffleChildren();
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            for (int i = 0; i < children.Count; i++)
            {
                switch (children[i].Update())
                {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        break;
                    case State.Success:
                        return State.Success;
                }
            }

            return State.Failure;
        }

        void ShuffleChildren()
        {
            System.Random rng = new System.Random();
            int n = children.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Node value = children[k];
                children[k] = children[n];
                children[n] = value;
            }
        }
    }
}