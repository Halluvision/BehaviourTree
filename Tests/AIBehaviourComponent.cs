using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Halluvision.BehaviourTree;

public class AIBehaviourComponent : BehaviourTreeComponent
{
    public Agent agent;

    private void Awake()
    {
        agent = GetComponent<Agent>();
    }
}
