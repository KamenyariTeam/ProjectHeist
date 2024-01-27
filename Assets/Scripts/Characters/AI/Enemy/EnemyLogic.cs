using System.Collections.Generic;
using Character;
using InteractableObjects.Door;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI.Enemy
{
    public class EnemyLogic : MonoBehaviour, IAILogic
    {
        public static readonly int ReloadAnimation = Animator.StringToHash("PlayerPlaceholder_HandGun_Reload");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        [SerializeField] private string playerTag;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float playerDetectionInterval;
        [SerializeField] private float idleSpeed;
        [SerializeField] private float viewDistance;
        [SerializeField] private float viewAngle;
        [SerializeField] private float guaranteedDetectDistance;
        [SerializeField] private float delayBeforeAttack;
        [SerializeField] private float searchingRotationSpeed;
        [SerializeField] private float searchingTotalRotation;
        [SerializeField] private WeaponComponent weapon;
        [SerializeField] private float shootingDistance;
        [SerializeField] private float chasingSpeed;
        [SerializeField] private Transform[] initialPath;

        private Rigidbody2D _rigidbody;
        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private Animator _animator;
        private bool _isDetectingPlayer;
        private float _timeTillPlayerDetection;
        private List<InteractableObjects.IInteractable> _activeInteracts;

        private AIState _state;
        private Dictionary<AIState, BaseEnemyState> _statesLogic;

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

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();

            _agent = GetComponent<NavMeshAgent>();
            _agent.updateUpAxis = false;
            _agent.updateRotation = false;


            // enemies check player at different time => should help to avoid freezes when all enemies
            // try to find the player
            _timeTillPlayerDetection = Random.Range(0, playerDetectionInterval);

            _activeInteracts = new List<InteractableObjects.IInteractable>();

            _playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;

            var idleState = new EnemyPatrollingState(initialPath, delayBeforeAttack, idleSpeed);
            var attackState = new EnemyAttackingState(weapon, chasingSpeed, shootingDistance);
            var chasingState = new EnemyChasingState(chasingSpeed);
            var searchingState = new EnemySearchingForPlayerState(searchingRotationSpeed, searchingTotalRotation);

            _statesLogic = new Dictionary<AIState, BaseEnemyState>
            {
                [AIState.Patrolling] = idleState,
                [AIState.Attacking] = attackState,
                [AIState.ChasingPlayer] = chasingState,
                [AIState.SearchingForPlayer] = searchingState
            };

            foreach (BaseEnemyState state in _statesLogic.Values)
            {
                state.Init(transform, _rigidbody, _agent, _animator, _playerTransform);
            }

            _state = AIState.Patrolling;
            _statesLogic[_state].OnStart();
        }

        private void Update()
        {
            Debug.Log(_state);
            AIState newState = _statesLogic[_state].OnUpdate(Time.deltaTime, _isDetectingPlayer);
            if (newState != _state)
            {
                _statesLogic[_state].OnStop();
                _state = newState;
                _statesLogic[_state].OnStart();
            }

            _timeTillPlayerDetection -= Time.deltaTime;
            if (_timeTillPlayerDetection <= 0.0f)
            {
                _isDetectingPlayer = DetectPlayer();
                _timeTillPlayerDetection = playerDetectionInterval;
            }

            CheckForDoors();

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
            var rigidbody = GetComponent<Rigidbody2D>();
            var position = transform.position;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(position, guaranteedDetectDistance);

            float originalAngle = rigidbody.rotation;
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
    }

    abstract class BaseEnemyState : IAIStateLogic
    {
        protected Transform _transform;
        protected Rigidbody2D _rigidBody;
        protected NavMeshAgent _agent;
        protected Transform _playerTransform;
        protected Animator _animator;

        public void Init(Transform transform, Rigidbody2D rigidbody, NavMeshAgent agent,
            Animator animator, Transform playerTransform)
        {
            _transform = transform;
            _rigidBody = rigidbody;
            _agent = agent;
            _playerTransform = playerTransform;
            _animator = animator;
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
            _agent.SetDestination(_path[_pathIndex].position);
            _agent.speed = _speed;
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            _agent.isStopped = isDetectingPlayer;

            Debug.Log(_path.Length);

            if (isDetectingPlayer)
            {
                Vector3 directionToPlayer = (_playerTransform.position - _transform.position).normalized;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                _rigidBody.rotation = angle;
                _timeTillAttack = Mathf.Max(_timeTillAttack - timeDelta, -1);
                if (_timeTillAttack < 0.0f)
                {
                    return AIState.Attacking;
                }
                return AIState.Patrolling;
            }

            _timeTillAttack = _attackDelay;

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _pathIndex = (_pathIndex + 1) % _path.Length;
                _agent.SetDestination(_path[_pathIndex].position);
            }
            else
            {
                var velocity = _agent.velocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                _rigidBody.rotation = angle;
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
            _agent.speed = _speed;
            _agent.SetDestination(_playerTransform.position);
        }

        public override AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            Vector3 directionToPlayer = (_playerTransform.position - _transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            _rigidBody.rotation = angle;

            if (isDetectingPlayer)
            {
                float distanceToPlayer = (_transform.position - _playerTransform.position).magnitude;
                if (distanceToPlayer < _shootingDistance)
                {
                    _agent.isStopped = true;
                    if (_weapon.CurrentAmmo == 0)
                    {
                        _weapon.Reload();
                        _animator.Play(EnemyLogic.ReloadAnimation);
                    }
                    else if (_weapon.CanShoot)
                    {
                        _weapon.Shoot();
                    }
                }
                else if (!_agent.pathPending)
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerTransform.position);
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
            _agent.SetDestination(_playerTransform.position);
            _agent.isStopped = false;
            _agent.speed = _speed;
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
                (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance))
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
            _agent.isStopped = true;
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
                _rigidBody.rotation += angleDelta;
                if (_rigidBody.rotation > 180.0f)
                {
                    _rigidBody.rotation -= 360.0f;
                }
            }
            else
            {
                _rigidBody.rotation -= angleDelta;
                if (_rigidBody.rotation < -180.0f)
                {
                    _rigidBody.rotation += 360.0f;
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

