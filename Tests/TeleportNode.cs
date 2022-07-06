using UnityEngine;
using Halluvision.BehaviourTree;

public class TeleportNode : ActionNode
{
    Vector2 targetPoint;

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

        if (_agent.Teleport(playerTransform.position))
            state = State.Success;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (state != State.Running)
            OnStop();
        return state;
    }
}
