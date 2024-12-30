using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI
{
    //TODO: reimplement using new weapon system

    [System.Serializable]
    public class AttackingState : BaseAIStateLogic
    {
        public interface IStateProperties : IPlayerDetector
        {
        }

        // Hashes for animator parameters
        //private static readonly int ReloadAnimation = Animator.StringToHash("PlayerPlaceholder_HandGun_Reload");
        //private WeaponComponent _weapon;

        [SerializeField] private AIState lostPlayerState = AIState.ChasingPlayer;
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float shootingDistance = 3.0f;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private IStateProperties _properties;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _properties = aiLogic as IStateProperties;
        }

        public override void OnEnter()
        {
            _properties.IsOnAlert = true;
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.SetDestination(_properties.PlayerTransform.position);
        }

        public override AIState OnUpdate(float deltaTime)
        {
            _rigidBody.rotation = AIHelpers.RotateToObject(_aiLogic.transform.position, _properties.PlayerTransform.position);

            if (_properties.IsPlayerInSight)
            {
                float distanceToPlayer = (_aiLogic.transform.position - _properties.PlayerTransform.position).magnitude;
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
                    _agent.SetDestination(_properties.PlayerTransform.position);
                }
                return _aiLogic.State;
            }

            return lostPlayerState;
        }
    }
}