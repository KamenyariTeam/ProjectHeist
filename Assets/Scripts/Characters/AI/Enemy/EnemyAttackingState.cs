using Characters.Player;
using UnityEngine;

namespace Characters.AI.Enemy
{
    public class EnemyAttackingState : BaseEnemyState
    {
        // Hashes for animator parameters
        private static readonly int ReloadAnimation = Animator.StringToHash("PlayerPlaceholder_HandGun_Reload");
        
        private WeaponComponent _weapon;
        private float _speed;
        private float _shootingDistance;

        public EnemyAttackingState(WeaponComponent weapon, float speed, float shootingDistance)
        {
            _weapon = weapon;
            _speed = speed;
            _shootingDistance = shootingDistance;
        }

        public override void OnEnter()
        {
            EnemyLogic.isOnAlert = true;
            EnemyLogic.Agent.isStopped = false;
            EnemyLogic.Agent.speed = _speed;
            EnemyLogic.Agent.SetDestination(EnemyLogic.PlayerTransform.position);
        }

        public override AIState OnUpdate(float deltaTime)
        {
            EnemyLogic.RotateToObject(EnemyLogic.PlayerTransform);

            if (EnemyLogic.IsPlayerInSight)
            {
                float distanceToPlayer = (EnemyLogic.transform.position - EnemyLogic.PlayerTransform.position).magnitude;
                if (distanceToPlayer < _shootingDistance)
                {
                    EnemyLogic.Agent.isStopped = true;
                    if (_weapon.CurrentAmmo == 0)
                    {
                        _weapon.Reload();
                        EnemyLogic.Animator.Play(ReloadAnimation);
                    }
                    else if (_weapon.CanShoot)
                    {
                        _weapon.Shoot();
                    }
                }
                else if (!EnemyLogic.Agent.pathPending)
                {
                    EnemyLogic.Agent.isStopped = false;
                    EnemyLogic.Agent.SetDestination(EnemyLogic.PlayerTransform.position);
                }
                return AIState.Attacking;
            }

            return AIState.ChasingPlayer;
        }
    }
}