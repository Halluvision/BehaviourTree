using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.BehaviourTree
{
    public abstract class ConditionNode : DecoratorNode
    {
        [Header("General Condition")]
        public bool AbortOnChangeResult;
        public AbortType abortType;
        public bool reverse = false;

        bool checkConditionAgain;

        protected override void OnStart()
        {
            checkConditionAgain = false;
            bool condition = OnConditionCheck();
            condition = reverse ? !condition : condition;
            if (condition)
            {
                state = State.Running;
                if (abortType == AbortType.LowerPriority || abortType == AbortType.Both)
                    tree.AbortActive(AbortType.LowerPriority, this);
            }
            else
            {
                state = State.Failure;
                if (abortType == AbortType.Self || abortType == AbortType.Both)
                    tree.AbortActive(AbortType.Self, this);
            }
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (state == State.Failure)
                return state;

            if (checkConditionAgain)
            {
                bool condition = OnConditionCheck();
                condition = reverse ? !condition : condition;

                if (!condition)
                    state = State.Failure;
            }
            
            checkConditionAgain = AbortOnChangeResult;
            
            if (state == State.Running)
                return child.Update();
            else if (state == State.Failure)
            {
                switch (abortType)
                {
                    case AbortType.None:
                        return state;
                    default:
                        tree.AbortActive(abortType, this);
                        return state;
                }
            }
            return State.Running;
        }

        protected abstract bool OnConditionCheck();
    }

    public enum AbortType
    {
        None,
        Self,
        LowerPriority,
        Both
    }
}