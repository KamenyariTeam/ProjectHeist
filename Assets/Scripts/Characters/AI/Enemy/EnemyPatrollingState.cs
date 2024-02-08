using UnityEngine;

namespace Characters.AI.Enemy
{
    public class EnemyPatrollingState : BaseEnemyState
    {
        private readonly Transform[] _path;
        private readonly float _speed;

        private int _pathIndex;
        
        public EnemyPatrollingState(Transform[] path, float speed)
        {
            _pathIndex = 0;
            _path = path;
            _speed = speed;
        }

        public override void OnEnter()
        {
            EnemyLogic.Agent.isStopped = false;
            EnemyLogic.Agent.SetDestination(_path[_pathIndex].position);
            EnemyLogic.Agent.speed = _speed;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (EnemyLogic.IsPlayerInSight && EnemyLogic.PlayerStealthComponent.IsNoticeable)
            {
                return EnemyLogic.isOnAlert ? AIState.Attacking : AIState.Suspicion;
            }

            EnemyLogic.DecreaseSuspicion(deltaTime);
            
            MoveAlongPath();

            return AIState.Patrolling;
        }

        private void MoveAlongPath()
        {
            if (!EnemyLogic.Agent.pathPending && EnemyLogic.Agent.remainingDistance <= EnemyLogic.Agent.stoppingDistance)
            {
                _pathIndex = (_pathIndex + 1) % _path.Length;
                EnemyLogic.Agent.SetDestination(_path[_pathIndex].position);
            }
            else
            {
                var velocity = EnemyLogic.Agent.velocity;
                var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                EnemyLogic.Rigidbody.rotation = angle;
            }
        }
    }
}