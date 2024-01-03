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
            OnAlarm
        }


        private static readonly float MinDistanceToPathPointMul = 2;
        private static readonly float MinDistanceToPathPointAdd = 0.2f;

        [SerializeField] private string playerTag;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float playerDetectionInterval;
        [SerializeField] private float viewDistance;
        [SerializeField] private float viewAngle;
        [SerializeField] private float guaranteedDetectDistance;
        [SerializeField] private Transform[] initialPath;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidbody;
        private int _pathIndex;
        private Transform _playerTransform;
        private State _state;
        private float _timeTillPlayerDetection;
        private bool _isDetectingPlayer;

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

        }

        // Update is called once per frame
        private void Update()
        {
            IdleLogic();


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
                Debug.Log("Too far");
                return false;
            }

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            float playerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            Debug.Log($"View {_rigidbody.rotation}, to player {playerAngle}");
            if (Mathf.Abs(_rigidbody.rotation - playerAngle) > 0.5f * viewAngle && viewDistance > guaranteedDetectDistance)
            {
                return false;
            }
            Debug.Log("linecase");
            return !Physics2D.Linecast(transform.position, _playerTransform.position, wallMask);   
        }

        private void IdleLogic()
        {
            _agent.isStopped = _isDetectingPlayer;

            if (_isDetectingPlayer)
            {
                Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                _rigidbody.rotation = angle;
                return;
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

        }

    }
}

