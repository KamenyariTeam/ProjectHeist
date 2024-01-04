using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Character
{

    class EnemyIdleState : IAIStateLogic
    {
        public Transform transform;
        public Rigidbody2D rigidbody;
        public NavMeshAgent agent;
        public Transform[] path;
        public Transform playerTransform;
        public float attackDelay;

        private int _pathIndex;
        private float _timeTillAttack;

        public EnemyIdleState()
        {
            _pathIndex = 0;
        }

        public void OnStart()
        {
            _timeTillAttack = attackDelay;
            agent.SetDestination(path[_pathIndex].position);
        }

        public void OnStop()
        {
        }

        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            agent.isStopped = isDetectingPlayer;

            if (isDetectingPlayer)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                rigidbody.rotation = angle;
                _timeTillAttack = Mathf.Max(_timeTillAttack - timeDelta, -1);
                if (_timeTillAttack < 0.0f)
                {
                    attackDelay = 0.0f;
                    return AIState.Attacking;
                }
                return AIState.Idle;
            }

            _timeTillAttack = attackDelay;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                _pathIndex = (_pathIndex + 1) % path.Length;
                agent.SetDestination(path[_pathIndex].position);
            }
            else
            {
                float angle = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
                rigidbody.rotation = angle;
            }

            agent.SetDestination(path[_pathIndex].position);

            return AIState.Idle;
        }
    }

    class EnemyAttackingState : IAIStateLogic
    {
        public Transform transform;
        public Rigidbody2D rigidbody;
        public NavMeshAgent agent;
        public Transform playerTransform;
        public WeaponComponent weapon;

        public void OnStart()
        {
        }

        public void OnStop()
        {
        }

        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            rigidbody.rotation = angle;

            if (isDetectingPlayer)
            {
                float distanceToPlayer = (transform.position - playerTransform.position).magnitude;
                if (distanceToPlayer < 0.8)
                {
                    agent.isStopped = true;
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
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                return AIState.Attacking;
            }

            return AIState.ChasingPlayer;
        }
    }

    class EnemyChasingState : IAIStateLogic
    {
        public Transform transform;
        public Rigidbody2D rigidbody;
        public NavMeshAgent agent;
        public Transform playerTransform;

        public void OnStart()
        {
            agent.SetDestination(playerTransform.position);
            agent.isStopped = false;
        }

        public void OnStop()
        {
        }

        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return AIState.SearchingForPlayer;
            }
            return AIState.ChasingPlayer;
        }
    }

    class EnemySearchingForPlayerState : IAIStateLogic
    {
        public Transform transform;
        public Rigidbody2D rigidbody;
        public NavMeshAgent agent;
        public Transform playerTransform;
        public float searchingRotationSpeed;

        private float _totalRotation;

        public void OnStart()
        {
            _totalRotation = 0;
            agent.isStopped = true;
        }

        public void OnStop()
        {
        }

        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer)
        {
            if (isDetectingPlayer)
            {
                return AIState.Attacking;
            }
            float angleDelta = searchingRotationSpeed * timeDelta;
            rigidbody.rotation += angleDelta;
            if (rigidbody.rotation > 180.0f)
            {
                rigidbody.rotation -= 360.0f;
            }

            _totalRotation += angleDelta;
            if (_totalRotation > 270.0f)
            {
                return AIState.Idle;
            }

            return AIState.SearchingForPlayer;
        }
    }



    public class EnemyLogic : MonoBehaviour, ICharacter
    {

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

        private Rigidbody2D _rigidbody;
        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private bool _isDetectingPlayer;
        private float _timeTillPlayerDetection;
        private List<InteractableObjects.IInteractable> _activeInteracts;

        private AIState _state;
        private Dictionary<AIState, IAIStateLogic> _statesLogic;

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


            // enemies check player at different time => should help to avoid freezes when all enemis
            // try to find the player
            _timeTillPlayerDetection = Random.Range(0, playerDetectionInterval);

            _activeInteracts = new List<InteractableObjects.IInteractable>();

            _playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;

            var idleState = new EnemyIdleState();
            idleState.transform = transform;
            idleState.rigidbody = _rigidbody;
            idleState.agent = _agent;
            idleState.attackDelay = delayBeforeAttack;
            idleState.playerTransform = _playerTransform;
            idleState.path = initialPath;
            

            var attackState = new EnemyAttackingState();
            attackState.agent = _agent;
            attackState.transform = transform;
            attackState.rigidbody = _rigidbody;
            attackState.playerTransform = _playerTransform;
            attackState.weapon = weapon;

            var chasingState = new EnemyChasingState();
            chasingState.agent = _agent;
            chasingState.transform = transform;
            chasingState.rigidbody = _rigidbody;
            chasingState.playerTransform = _playerTransform;

            var searchingState = new EnemySearchingForPlayerState();
            searchingState.playerTransform = _playerTransform;
            searchingState.rigidbody = _rigidbody;
            searchingState.searchingRotationSpeed = searchingRotationSpeed;
            searchingState.transform = transform;
            searchingState.agent = _agent;

            _statesLogic = new Dictionary<AIState, IAIStateLogic>();
            _statesLogic[AIState.Idle] = idleState;
            _statesLogic[AIState.Attacking] = attackState;
            _statesLogic[AIState.ChasingPlayer] = chasingState;
            _statesLogic[AIState.SearchingForPlayer] = searchingState;

            _state = AIState.Idle;
            _statesLogic[_state].OnStart();
        }

        // Update is called once per frame
        private void Update()
        {
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
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var interactable = collider.GetComponent<InteractableObjects.IInteractable>();
            if (interactable == null)
            {
                return;
            }
            _activeInteracts.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            var interactable = collider.GetComponent<InteractableObjects.IInteractable>();
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
                var door = interactable as InteractableObjects.Door;
                if (door == null)
                {
                    continue;
                }
                Vector3 directionToDoor = (door.transform.position - transform.position).normalized;
                float dotProduct = directionToDoor.x * _agent.velocity.x + directionToDoor.y * _agent.velocity.y;
                if(dotProduct > 0)
                {
                    door.Interact(gameObject);
                }
                Debug.Log(dotProduct);
                
            }
        }

    }
}

