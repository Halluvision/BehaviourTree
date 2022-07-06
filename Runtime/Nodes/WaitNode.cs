using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        public float randomness = 0f;
        
        float timer;
        float finishTime;
        protected override void OnStart()
        {
            timer = 0;
            finishTime = duration + Random.Range(-randomness, randomness);
        }

        protected override void OnStop()
        {
            started = false;
        }

        protected override State OnUpdate()
        {
            timer += Time.deltaTime;
            if (timer > finishTime)
            {
                started = false;
                return State.Success;
            }
                
            return State.Running;
        }
    }
}
