using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public class RepeatNode : DecoratorNode
    {
        public int loopCount = 0;
        public bool repeatDecpieSuccess = true;
        public bool repeatDecpieFailure = true;
        int counter;
        protected override void OnStart()
        {
            counter = 0;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            if (loopCount == 0)
            {
                child.Update();
                return State.Running;
            }

            if (counter <= loopCount - 1)
            {
                switch (child.Update())
                {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        counter++;
                        if (repeatDecpieFailure)
                            return State.Running;
                        else
                            return State.Failure;
                    case State.Success:
                        counter++;
                        if (repeatDecpieSuccess)
                            return State.Running;
                        else
                            return State.Success;
                    default:
                        return State.Failure;
                }
            }
            else 
                return base.state;
        }
    }
}