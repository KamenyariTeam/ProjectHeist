using System.Collections.Generic;
using Characters.Player;
using InteractableObjects.Door;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Characters.AI.Enemy
{
    public class EnemyLogic : MonoBehaviour, IAILogic
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        // Serialized fields grouped for clarity
        [Header("Detection Settings")]
        [SerializeField] private string playerTag;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float playerDetectionInterval;
        [SerializeField] private float viewDistance;
        [SerializeField] private float viewAngle;
        [SerializeField] private float guaranteedDetectDistance;
        [SerializeField] private float suspicionIncreaseRate = 0.1f; // Rate at which suspicion increases


        [Header("Movement Settings")]
        [SerializeField] private float idleSpeed;
        [SerializeField] private float chasingSpeed;

        [Header("Combat Settings")]
        [SerializeField] private WeaponComponent weapon;
        [SerializeField] private float shootingDistance;
        [SerializeField] private float delayBeforeAttack;

        [Header("Search Settings")]
        [SerializeField] private float searchingRotationSpeed;
        [SerializeField] private float searchingTotalRotation;

        [Header("Patrolling Settings")]
        [SerializeField] private Transform[] initialPath;

        private Rigidbody2D _rigidbody;
        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private Animator _animator;
        private HealthComponent _healthComponent;
        private bool _isDetectingPlayer;
        private float _timeTillPlayerDetection;
        private List<InteractableObjects.IInteractable> _activeInteracts;
        private AIState _state;
        private Dictionary<AIState, BaseEnemyState> _statesLogic;
        private float _suspicionLevel;

        public AIState State
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }
                _statesLogic[_state].OnStop();
                _state = value;
                _statesLogic[_state].OnStart();
            }
        }

        public bool HasState(AIState state)
        {
            return _statesLogic.ContainsKey(state);
        }

        public bool CanContact(IAILogic other)
        {
            EnemyLogic enemy = other as EnemyLogic;
            if (enemy == null)
            {
                return false;
            }

            return (transform.position - enemy.transform.position).magnitude < viewDistance;
        }

        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
        }

        private void InitializeComponents()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateUpAxis = false;
            _agent.updateRotation = false;

            _playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;
            _activeInteracts = new List<InteractableObjects.IInteractable>();

            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHendler;
        }

        private void InitializeStates()
        {
            _statesLogic = new Dictionary<AIState, BaseEnemyState>
            {
                [AIState.Patrolling] = new EnemyPatrollingState(initialPath, delayBeforeAttack, idleSpeed),
                [AIState.Attacking] = new EnemyAttackingState(weapon, chasingSpeed, shootingDistance),
                [AIState.ChasingPlayer] = new EnemyChasingState(chasingSpeed),
                [AIState.SearchingForPlayer] = new EnemySearchingForPlayerState(searchingRotationSpeed, searchingTotalRotation)
            };

            foreach (BaseEnemyState state in _statesLogic.Values)
            {
                state.Init(transform, _rigidbody, _agent, _animator, _playerTransform);
            }

            State = AIState.Patrolling;
        }

        private void Update()
        {
            _timeTillPlayerDetection -= Time.deltaTime;
            if (_timeTillPlayerDetection <= 0.0f)
            {
                _isDetectingPlayer = DetectPlayer();
                _timeTillPlayerDetection = playerDetectionInterval;
            }

            CheckForDoors();
            UpdateState();
            UpdateAnimator();
            
            // Check if the player is detectable and noticeable
            if (_isDetectingPlayer && PlayerIsNoticeable())
            {
                IncreaseSuspicion();
            }

            // Transition to attack state if suspicion reaches 1
            if (_suspicionLevel >= 1.0f)
            {
                State = AIState.Attacking;
                _suspicionLevel = 0; // Reset suspicion level
            }
        }

        private void UpdateState()
        {
            AIState newState = _statesLogic[_state].OnUpdate(Time.deltaTime, _isDetectingPlayer);
            if (newState != _state)
            {
                State = newState;
            }
        }

        private void UpdateAnimator()
        {
            _animator.SetBool(IsMoving, _agent.velocity != Vector3.zero);
        }

        private void OnTriggerEnter2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<InteractableObjects.IInteractable>();
            if (interactable == null)
            {
                return;
            }
            _activeInteracts.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<InteractableObjects.IInteractable>();
            if (interactable == null)
            {
                return;
            }
            _activeInteracts.Remove(interactable);
        }

        private bool DetectPlayer()
        {
            float distanceToPlayer = (transform.position - _playerTransform.position).magnitude;
            if (distanceToPlayer > viewDistance)
            {
                return false;
            }

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            float playerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(_rigidbody.rotation - playerAngle) > 0.5f * viewAngle && viewDistance > guaranteedDetectDistance)
            {
                return false;
            }
            return !Physics2D.Linecast(transform.position, _playerTransform.position, wallMask);
        }
        
        // Method to increase suspicion based on the player's noticeability
        private void IncreaseSuspicion()
        {
            StealthComponent playerStealth = _playerTransform.GetComponent<StealthComponent>();
            if (playerStealth)
            {
                // Increase suspicion more quickly if the player is more noticeable
                _suspicionLevel += Time.deltaTime * suspicionIncreaseRate * (playerStealth.noticeability / 20.0f);
                _suspicionLevel = Mathf.Clamp(_suspicionLevel, 0.0f, 1.0f); // Ensure suspicion stays within bounds
            }
        }

        // Checks if the player is in a detectable state
        private bool PlayerIsNoticeable()
        {
            StealthComponent playerStealth = _playerTransform.GetComponent<StealthComponent>();
            return playerStealth.IsNoticeable;
        }

        private void CheckForDoors()
        {
            foreach (InteractableObjects.IInteractable interactable in _activeInteracts)
            {
                var door = interactable as Door;

                if (door)
                {
                    Vector3 directionToDoor = (door.transform.position - transform.position).normalized;
                    var velocity = _agent.velocity;
                    float dotProduct = directionToDoor.x * velocity.x + directionToDoor.y * velocity.y;
                    if (dotProduct > 0)
                    {
                        door.Interact(gameObject);
                    }
                    Debug.Log(dotProduct);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var rb = GetComponent<Rigidbody2D>();
            var position = transform.position;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(position, guaranteedDetectDistance);

            float originalAngle = rb.rotation;
            int iterations = 10;
            float stepAngles = viewAngle / iterations;
            float angle = originalAngle - 0.5f * viewAngle;

            Vector3 pos = position;
            for (int i = 0; i <= iterations; i++)
            {
                float rad = Mathf.Deg2Rad * angle;
                Vector3 secondPos = transform.position + new Vector3(viewDistance * Mathf.Cos(rad), viewDistance * Mathf.Sin(rad), 0);
                Gizmos.DrawLine(pos, secondPos);

                angle += stepAngles;
                pos = secondPos;
            }
            Gizmos.DrawLine(pos, transform.position);
        }

        private void OnDeathHendler()
        {
            Destroy(gameObject);
        }
    }

    abstract class BaseEnemyState : IAIStateLogic
    {
        protected Transform Transform;
        protected Rigidbody2D RigidBody;
        protected NavMeshAgent Agent;
        protected Transform PlayerTransform;
        protected Animator Animator;

        public void Init(Transform transform, Rigidbody2D rigidbody, NavMeshAgent agent,
            Animator animator, Transform playerTransform)
        {
            Transform = transform;
            RigidBody = rigidbody;
            Agent = agent;
            PlayerTransform = playerTransform;
            Animator = animator;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnStop()
        {
        }

        public virtual AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            return AIState.Patrolling;
        }
    }

    class EnemyPatrollingState : BaseEnemyState
    {
        private Transform[] _path;
        private float _attackDelay;
        private float _speed;

        private int _pathIndex;
        private float _timeTillAttack;

        public EnemyPatrollingState(Transform[] path, float attackDelay, float speed)
        {
            _pathIndex = 0;
            _path = path;
            _attackDelay = attackDelay;
            _speed = speed;
        }

        public override void OnStart()
        {
            _timeTillAttack = _attackDelay;
            Agent.SetDestination(_path[_pathIndex].position);
            Agent.speed = _speed;
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            Agent.isStopped = isDetectingPlayer;

            //Debug.Log(_path.Length);

            if (isDetectingPlayer)
            {
                Vector3 directionToPlayer = (PlayerTransform.position - Transform.position).normalized;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                RigidBody.rotation = angle;
                _timeTillAttack = Mathf.Max(_timeTillAttack - timeDelta, -1);
                if (_timeTillAttack < 0.0f)
                {
                    return AIState.Attacking;
                }
                return AIState.Patrolling;
            }

            _timeTillAttack = _attackDelay;

            if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                _pathIndex = (_pathIndex + 1) % _path.Length;
                Agent.SetDestination(_path[_pathIndex].position);
            }
            else
            {
                var velocity = Agent.velocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                RigidBody.rotation = angle;
            }

            return AIState.Patrolling;
        }

        public override void OnStop()
        {
            _attackDelay = 0.0f;
        }
    }

    class EnemyAttackingState : BaseEnemyState
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

        public override void OnStart()
        {
            Agent.speed = _speed;
            Agent.SetDestination(PlayerTransform.position);
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            Vector3 directionToPlayer = (PlayerTransform.position - Transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            RigidBody.rotation = angle;

            if (isDetectingPlayer)
            {
                float distanceToPlayer = (Transform.position - PlayerTransform.position).magnitude;
                if (distanceToPlayer < _shootingDistance)
                {
                    Agent.isStopped = true;
                    if (_weapon.CurrentAmmo == 0)
                    {
                        _weapon.Reload();
                        Animator.Play(ReloadAnimation);
                    }
                    else if (_weapon.CanShoot)
                    {
                        _weapon.Shoot();
                    }
                }
                else if (!Agent.pathPending)
                {
                    Agent.isStopped = false;
                    Agent.SetDestination(PlayerTransform.position);
                }
                return AIState.Attacking;
            }

            return AIState.ChasingPlayer;
        }
    }

    class EnemyChasingState : BaseEnemyState
    {
        private const float MaxTimeInState = 10.0f;

        private float _speed;

        private float _timeInState;

        public EnemyChasingState(float speed)
        {
            _speed = speed;
        }

        public override void OnStart()
        {
            Agent.SetDestination(PlayerTransform.position);
            Agent.isStopped = false;
            Agent.speed = _speed;
            _timeInState = 0;
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            if (isDetectingPlayer)
            {
                return AIState.Attacking;
            }
            _timeInState += timeDelta;
            if ((_timeInState >= MaxTimeInState) ||
                (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance))
            {
                return AIState.SearchingForPlayer;
            }
            return AIState.ChasingPlayer;
        }
    }

    class EnemySearchingForPlayerState : BaseEnemyState
    {
        private float _searchingRotationSpeed;
        private float _totalRotationAngle;

        private float _totalRotation;

        public EnemySearchingForPlayerState(float searchingRotationSpeed, float totalRotationAngle)
        {
            _searchingRotationSpeed = searchingRotationSpeed;
            _totalRotationAngle = totalRotationAngle;
        }

        public override void OnStart()
        {
            _totalRotation = 0;
            Agent.isStopped = true;
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            if (isDetectingPlayer)
            {
                return AIState.Attacking;
            }
            float angleDelta = _searchingRotationSpeed * timeDelta;
            if (_totalRotation < _totalRotationAngle)
            {
                RigidBody.rotation += angleDelta;
                if (RigidBody.rotation > 180.0f)
                {
                    RigidBody.rotation -= 360.0f;
                }
            }
            else
            {
                RigidBody.rotation -= angleDelta;
                if (RigidBody.rotation < -180.0f)
                {
                    RigidBody.rotation += 360.0f;
                }
            }


            _totalRotation += angleDelta;
            if (_totalRotation > 3.0f * _totalRotationAngle)
            {
                return AIState.Patrolling;
            }

            return AIState.SearchingForPlayer;
        }
    }
}

