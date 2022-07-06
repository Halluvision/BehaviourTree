using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Halluvision.BehaviourTree;

public class MoveToPlayerNode : ActionNode
{
    [SerializeField]
    float _maxDistance = 4f;

    Agent _agent;
    Transform playerTransform;

    protected override void OnStart()
    {
        var comp = behaviourTreeComponent as AIBehaviourComponent;
        _agent = comp.agent;

        playerTransform = PlayerMove._transform;
        if (playerTransform == null || _agent == null)
        {
            state = State.Failure;
            return;
        }
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (playerTransform != null)
        {
            if (_agent.MoveToPoint(playerTransform.position, _maxDistance))
                return State.Running;
            else
            {
                OnStop();
                return State.Success;
            }
        }
        OnStop();
        return State.Failure;
    }
}
