using Characters.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI.Enemy
{
    //TODO: reimplement using new weapon system

    [System.Serializable]
    public class AttackingState : BaseAIStateLogic
    {
        // Hashes for animator parameters
        //private static readonly int ReloadAnimation = Animator.StringToHash("PlayerPlaceholder_HandGun_Reload");

        //private WeaponComponent _weapon;

        [SerializeField] private AIState lostPlayerState = AIState.ChasingPlayer;
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float shootingDistance = 3.0f;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private IAIPlayerDetectable _playerDetector;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _playerDetector = aiLogic as IAIPlayerDetectable;
        }

        public override void OnEnter()
        {
            _playerDetector.IsOnAlert = true;
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.SetDestination(_playerDetector.PlayerTransform.position);
        }

        public override AIState OnUpdate(float deltaTime)
        {
            _rigidBody.rotation = AIHelpers.RotateToObject(_aiLogic.transform.position, _playerDetector.PlayerTransform.position);

            if (_playerDetector.IsPlayerInSight)
            {
                float distanceToPlayer = (_aiLogic.transform.position - _playerDetector.PlayerTransform.position).magnitude;
                if (distanceToPlayer < shootingDistance)
                {
                    _agent.isStopped = true;
                    //if (_weapon.equippedWeapon.CurrentAmmo == 0)
                    //{
                    //    _weapon.Reload();
                    //    EnemyLogic.Animator.Play(ReloadAnimation);
                    //}
                    //else if (_weapon.equippedWeapon.CanAttack)
                    //{
                    //    _weapon.StartFire();
                    //}
                }
                else if (!_agent.pathPending)
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerDetector.PlayerTransform.position);
                }
                return _aiLogic.State;
            }

            return lostPlayerState;
        }
    }
}