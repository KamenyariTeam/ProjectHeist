using UnityEngine;

namespace Characters.AI.Enemy
{
    public class EnemySearchingForPlayerState : BaseEnemyState
    {
        private readonly float _searchingRotationSpeed;
        private readonly float _totalRotationAngle;

        private float _totalRotation;

        public EnemySearchingForPlayerState(float searchingRotationSpeed, float totalRotationAngle)
        {
            _searchingRotationSpeed = searchingRotationSpeed;
            _totalRotationAngle = totalRotationAngle;
        }

        public override void OnEnter()
        {
            _totalRotation = 0;
            EnemyLogic.Agent.isStopped = true;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (EnemyLogic.IsPlayerInSight)
            {
                return AIState.Attacking;
            }
            float angleDelta = _searchingRotationSpeed * deltaTime;
            if (_totalRotation < _totalRotationAngle)
            {
                EnemyLogic.Rigidbody.rotation += angleDelta;
                if (EnemyLogic.Rigidbody.rotation > 180.0f)
                    EnemyLogic.Rigidbody.rotation -= 360.0f;
            }
            else
            {
                EnemyLogic.Rigidbody.rotation -= angleDelta;
                if (EnemyLogic.Rigidbody.rotation < -180.0f) 
                    EnemyLogic.Rigidbody.rotation += 360.0f;
            }

            _totalRotation += angleDelta;
            return _totalRotation > 3.0f * _totalRotationAngle ? AIState.Patrolling : AIState.SearchingForPlayer;
        }
    }
}