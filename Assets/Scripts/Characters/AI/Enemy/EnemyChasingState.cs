using UnityEngine;

namespace Characters.AI.Enemy
{
    public class EnemyChasingState : BaseEnemyState
    {
        private const float MaxTimeInState = 10.0f;
        private float _speed;
        private float _timeInState;

        public EnemyChasingState(float speed)
        {
            _speed = speed;
        }

        public override void OnEnter()
        {
            EnemyLogic.Agent.SetDestination(EnemyLogic.PlayerTransform.position);
            EnemyLogic.Agent.isStopped = false;
            EnemyLogic.Agent.speed = _speed;
            _timeInState = 0;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (EnemyLogic.IsPlayerInSight)
            {
                return AIState.Attacking;
            }

            _timeInState += deltaTime;
            if (_timeInState >= MaxTimeInState ||
                (!EnemyLogic.Agent.pathPending && EnemyLogic.Agent.remainingDistance <= EnemyLogic.Agent.stoppingDistance))
            {
                return AIState.SearchingForPlayer;
            }

            return AIState.ChasingPlayer;
        }
    }
}