using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    [System.Serializable]
    public class Blackboard : ScriptableObject
    {
        public Vector2 moveDirection;
        public float detectPlayerRadius = 3;
        public float losePlayerRadius = 6;
        public Vector3 teleportPosition;
        public GameObject teleportTarget;
        public Vector2 eqsResult;
    }
}