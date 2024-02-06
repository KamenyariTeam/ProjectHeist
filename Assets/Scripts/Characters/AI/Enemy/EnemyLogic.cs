using System.Collections.Generic;
using Characters.Player;
using InteractableObjects.Door;
using UI;
using UnityEngine;
using UnityEngine.AI;

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
        [SerializeField] private float suspicionIncreaseRate = 0.1f;
        [SerializeField] private SuspicionMeter suspicionMeter;

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
        private StealthComponent _playerStealthComponent;
        private Animator _animator;
        private bool _isPlayerInSight;
        private float _cooldownPlayerDetection;
        private HealthComponent _healthComponent;
        private bool _isDetectingPlayer;
        private float _timeTillPlayerDetection;
        
        private List<InteractableObjects.IInteractable> _activeInteracts;
        private AIState _state;
        private Dictionary<AIState, BaseEnemyState> _statesLogic;
        
        public Rigidbody2D Rigidbody => _rigidbody;
        public NavMeshAgent Agent => _agent;
        public Transform PlayerTransform => _playerTransform;
        public StealthComponent PlayerStealthComponent => _playerStealthComponent;
        public Animator Animator => _animator;
        public SuspicionMeter EnemySuspicionMeter => suspicionMeter;
        public bool IsPlayerInSight => _isPlayerInSight;

        public AIState State
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }
                _statesLogic[_state].OnExit();
                _state = value;
                _statesLogic[_state].OnEnter();
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
        
        public void RotateToObject(Transform objectTransform)
        {
            Vector3 direction = (objectTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
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

            var player = GameObject.FindGameObjectWithTag(playerTag);
            _playerTransform = player.transform;
            _playerStealthComponent = player.GetComponent<StealthComponent>();
            _activeInteracts = new List<InteractableObjects.IInteractable>();

            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHandler;
        }

        private void InitializeStates()
        {
            _statesLogic = new Dictionary<AIState, BaseEnemyState>
            {
                [AIState.Patrolling] = new EnemyPatrollingState(initialPath, delayBeforeAttack, idleSpeed, suspicionIncreaseRate),
                [AIState.Attacking] = new EnemyAttackingState(weapon, chasingSpeed, shootingDistance),
                [AIState.ChasingPlayer] = new EnemyChasingState(chasingSpeed),
                [AIState.SearchingForPlayer] = new EnemySearchingForPlayerState(searchingRotationSpeed, searchingTotalRotation)
            };

            foreach (BaseEnemyState state in _statesLogic.Values)
            {
                state.Init(this);
            }

            State = AIState.Patrolling;
        }

        private void Update()
        {
            _cooldownPlayerDetection -= Time.deltaTime;
            if (_cooldownPlayerDetection <= 0.0f)
            {
                _isPlayerInSight = CheckIfPlayerInSight();
                _cooldownPlayerDetection = playerDetectionInterval;
            }

            CheckForDoors();
            UpdateState();
            UpdateAnimator();
        }

        private void UpdateState()
        {
            AIState newState = _statesLogic[_state].OnUpdate();
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

        private bool CheckIfPlayerInSight()
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

        private void OnDeathHandler()
        {
            Destroy(gameObject);
        }
    }

    internal abstract class BaseEnemyState : IAIStateLogic
    {
        protected EnemyLogic EnemyLogic;

        public virtual void Init(EnemyLogic enemyLogic)
        {
            EnemyLogic = enemyLogic;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual AIState OnUpdate()
        {
            return AIState.Patrolling;
        }
    }

    internal class EnemyPatrollingState : BaseEnemyState
    {
        private readonly Transform[] _path;
        private readonly float _speed;
        private readonly float _suspicionIncreaseRate;

        private int _pathIndex;
        
        private float _suspicionLevel;
        private bool _isOnAlert;
        private float _attackDelay;
        private float _timeTillAttack;

        public EnemyPatrollingState(Transform[] path, float attackDelay, float speed, float suspicionIncreaseRate)
        {
            _pathIndex = 0;
            _path = path;
            _attackDelay = attackDelay;
            _speed = speed;
            _suspicionIncreaseRate = suspicionIncreaseRate;
        }

        public override void OnEnter()
        {
            _timeTillAttack = _attackDelay;
            EnemyLogic.Agent.SetDestination(_path[_pathIndex].position);
            EnemyLogic.Agent.speed = _speed;
        }

        public override AIState OnUpdate()
        {
            if (HandleVisibilityAndSuspicion(out var onUpdate))
            {
                return onUpdate;
            }

            _timeTillAttack = _attackDelay;

            MoveAlongPath();

            return AIState.Patrolling;
        }

        private bool HandleVisibilityAndSuspicion(out AIState onUpdate)
        {
            EnemyLogic.Agent.isStopped = false;

            if (EnemyLogic.IsPlayerInSight)
            {
                if (!_isOnAlert)
                {
                    EnemyLogic.Agent.isStopped = EnemyLogic.PlayerStealthComponent.IsNoticeable;

                    if (EnemyLogic.PlayerStealthComponent.IsNoticeable)
                    {
                        EnemyLogic.RotateToObject(EnemyLogic.PlayerTransform);

                        IncreaseSuspicion();

                        // Transition to attack state if suspicion reaches 1
                        if (_suspicionLevel >= 1.0f)
                        {
                            _timeTillAttack = Mathf.Max(_timeTillAttack - Time.deltaTime, -1);
                            if (_timeTillAttack < 0.0f)
                            {
                                _isOnAlert = true;
                                EnemyLogic.EnemySuspicionMeter.SetVisibility(false);

                                onUpdate = AIState.Attacking;
                                return true;
                            }
                        }

                        onUpdate = AIState.Patrolling;
                        return true;
                    }

                    DecreaseSuspicion();
                }
                else
                {
                    onUpdate = AIState.Attacking;
                    return true;
                }
            }

            DecreaseSuspicion();

            onUpdate = AIState.Patrolling;
            return false;
        }

        public override void OnExit()
        {
            _attackDelay = 0.0f;
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

        private void IncreaseSuspicion()
        {
            EnemyLogic.EnemySuspicionMeter.SetVisibility(true);

            // Increase suspicion more quickly if the player is more noticeable
            _suspicionLevel += Time.deltaTime * _suspicionIncreaseRate * EnemyLogic.PlayerStealthComponent.Noticeability;
            _suspicionLevel = Mathf.Clamp(_suspicionLevel, 0.0f, 1.0f);
            EnemyLogic.EnemySuspicionMeter.SetBarFillAmount(_suspicionLevel);
        }

        private void DecreaseSuspicion()
        {
            if (_suspicionLevel > 0.0f)
            {
                _suspicionLevel -= Time.deltaTime * _suspicionIncreaseRate;
                _suspicionLevel = Mathf.Clamp(_suspicionLevel, 0.0f, 1.0f);
                EnemyLogic.EnemySuspicionMeter.SetBarFillAmount(_suspicionLevel);
            }
            else
            {
                EnemyLogic.EnemySuspicionMeter.SetVisibility(false);
            }
        }
    }

    internal class EnemyAttackingState : BaseEnemyState
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
            EnemyLogic.Agent.speed = _speed;
            EnemyLogic.Agent.SetDestination(EnemyLogic.PlayerTransform.position);
        }

        public override AIState OnUpdate()
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

    internal class EnemyChasingState : BaseEnemyState
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

        public override AIState OnUpdate()
        {
            if (EnemyLogic.IsPlayerInSight)
            {
                return AIState.Attacking;
            }

            _timeInState += Time.deltaTime;
            if (_timeInState >= MaxTimeInState ||
                (!EnemyLogic.Agent.pathPending && EnemyLogic.Agent.remainingDistance <= EnemyLogic.Agent.stoppingDistance))
            {
                return AIState.SearchingForPlayer;
            }

            return AIState.ChasingPlayer;
        }
    }

    internal class EnemySearchingForPlayerState : BaseEnemyState
    {
        private float _searchingRotationSpeed;
        private float _totalRotationAngle;

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

        public override AIState OnUpdate()
        {
            if (EnemyLogic.IsPlayerInSight)
            {
                return AIState.Attacking;
            }
            float angleDelta = _searchingRotationSpeed * Time.deltaTime;
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
            if (_totalRotation > 3.0f * _totalRotationAngle) 
                return AIState.Patrolling;

            return AIState.SearchingForPlayer;
        }
    }
}

