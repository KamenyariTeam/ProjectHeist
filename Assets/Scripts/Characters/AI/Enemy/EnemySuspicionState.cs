﻿namespace Characters.AI.Enemy
{
    public class EnemySuspicionState : BaseEnemyState
    {
        private readonly float _suspicionIncreaseRate;
        
        public EnemySuspicionState(float suspicionIncreaseRate)
        {
            _suspicionIncreaseRate = suspicionIncreaseRate;
        }
        
        public override void OnEnter()
        {
            EnemyLogic.Agent.isStopped = true;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (EnemyLogic.IsPlayerInSight && EnemyLogic.PlayerStealthComponent.IsNoticeable)
            {
                if (EnemyLogic.isOnAlert)
                {
                    return AIState.Attacking;
                }
                
                EnemyLogic.RotateToObject(EnemyLogic.PlayerTransform);
                EnemyLogic.ChangeSuspicionLevel(deltaTime * _suspicionIncreaseRate * EnemyLogic.PlayerStealthComponent.Noticeability);

                // Transition to attack state if suspicion reaches 1
                if (EnemyLogic.SuspicionLevel >= 1.0f)
                {
                    return AIState.Attacking;
                }
            }
            else
            {
                return AIState.Patrolling;
            }
            
            return AIState.Suspicion;
        }
    }
}