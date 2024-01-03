using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    public class EnemyLogic : MonoBehaviour, ICharacter
    {
        private enum State
        {
            Idle,
            Attacking,
            OnAlarm,
            ChasingPlayer,
            SearchingForPlayer
        }


        private static readonly float MinDistanceToPathPointMul = 2;
        private static readonly float MinDistanceToPathPointAdd = 0.2f;

        [SerializeField] private string playerTag;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float playerDetectionInterval;
        [SerializeField] private float viewDistance;
        [SerializeField] private float viewAngle;
        [SerializeField] private float guaranteedDetectDistance;
        [SerializeField] private float delayBeforeAttack;
        [SerializeField] private float searchingRotationSpeed;
        [SerializeField] private WeaponComponent weapon;
        [SerializeField] private Transform[] initialPath;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidbody;
        private int _pathIndex;
        private Transform _playerTransform;
        private State _state;
        private float _timeTillPlayerDetection;
        private bool _isDetectingPlayer;

        private float _idleAttackTimer;
        private float _searchTotalRotation;
        private Vector3 _playerLastPosition;


        public CharacterType GetCharacterType()
        {
            return CharacterType.Enemy;
        }

        // Start is called before the first frame update
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _agent = GetComponent<NavMeshAgent>();
            _agent.updateUpAxis = false;
            _agent.updateRotation = false;

            _state = State.Idle;

            _playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;

            _pathIndex = 0;
            _agent.SetDestination(initialPath[_pathIndex].position);

            // enemies check player at different time => should help to avoid freezes when all enemis
            // try to find the player
            _timeTillPlayerDetection = Random.Range(0, playerDetectionInterval);
            _idleAttackTimer = delayBeforeAttack;
        }

        // Update is called once per frame
        private void Update()
        {
            switch (_state)
            {
                case State.Idle:
                case State.OnAlarm:
                    _state = PatrollingLogic(Time.deltaTime);
                    break;
                case State.Attacking:
                    _state = AttackingLogic();
                    break;
                case State.ChasingPlayer:
                    _state = ChasingLogic();
                    break;
                case State.SearchingForPlayer:
                    _state = SearchingForPlayerLogic(Time.deltaTime);
                    break;
            }

            _timeTillPlayerDetection -= Time.deltaTime;
            if (_timeTillPlayerDetection <= 0.0f)
            {
                _isDetectingPlayer = DetectPlayer();
                _timeTillPlayerDetection = playerDetectionInterval;
                Debug.Log(_isDetectingPlayer);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var interactable = collider.GetComponent<InteractableObjects.IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
            }
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

        private State PatrollingLogic(float timeDelta)
        {
            _agent.isStopped = _isDetectingPlayer;

            if (_isDetectingPlayer)
            {
                Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                _rigidbody.rotation = angle;
                _idleAttackTimer = Mathf.Max(_idleAttackTimer - timeDelta, -1);
                if (_idleAttackTimer < 0.0f)
                {
                    return State.Attacking;
                }
                return _state;
            }

            if (_state == State.Idle)
            {
                _idleAttackTimer = delayBeforeAttack;
            }

            if (_agent.velocity != Vector3.zero)
            {
                float angle = Mathf.Atan2(_agent.velocity.y, _agent.velocity.x) * Mathf.Rad2Deg;
                _rigidbody.rotation = angle;
            }

            float distanceToPathPoint = (transform.position - initialPath[_pathIndex].position).magnitude;
            if (distanceToPathPoint < _agent.stoppingDistance * MinDistanceToPathPointMul + MinDistanceToPathPointAdd)
            {
                _pathIndex = (_pathIndex + 1) % initialPath.Length;
                _agent.SetDestination(initialPath[_pathIndex].position);
            }

            _agent.SetDestination(initialPath[_pathIndex].position);

            return _state;

        }

        private State AttackingLogic()
        {
            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;

            if (_isDetectingPlayer)
            {
                float distanceToPlayer = (transform.position - _playerTransform.position).magnitude;
                if (distanceToPlayer < 0.8)
                {
                    _agent.isStopped = true;
                    if (weapon.CurrentAmmo == 0)
                    {
                        weapon.Reload();
                    }
                    else if (weapon.CanShoot)
                    {
                        weapon.Shoot();
                    }
                }
                else
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerTransform.position);
                }
                return State.Attacking;
            }
            _playerLastPosition = _playerTransform.position;
            _agent.SetDestination(_playerLastPosition);
            _agent.isStopped = false;
            return State.ChasingPlayer;
        }

        private State ChasingLogic()
        {
            float distanceToLastPosition = (transform.position - _playerLastPosition).magnitude;
            if (distanceToLastPosition < _agent.stoppingDistance * MinDistanceToPathPointMul + MinDistanceToPathPointAdd)
            {
                _searchTotalRotation = 0;
                return State.SearchingForPlayer;
            }
            return State.ChasingPlayer;
        }

        private State SearchingForPlayerLogic(float timeDelta)
        {
            _agent.isStopped = true;
            if (_isDetectingPlayer)
            {
                return State.Attacking;
            }
            float angleDelta = searchingRotationSpeed * timeDelta;
            _rigidbody.rotation += angleDelta;
            if (_rigidbody.rotation > 180.0f)
            {
                _rigidbody.rotation -= 360.0f;
            }

            _searchTotalRotation += angleDelta;
            if (_searchTotalRotation > 270.0f)
            {
                return State.OnAlarm;
            }

            return State.SearchingForPlayer;
        }

    }
}

