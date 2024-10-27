using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using GameManagers;
using InteractableObjects.Door;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI.Enemy
{
    public class EnemyLogic : BaseAILogic,
        AttackingState.IStateProperties,
        ChasingState.IStateProperties,
        PatrollingState.IStateProperties,
        SearchingForPlayerState.IStateProperties,
        SuspicionState.IStateProperties
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        // Serialized fields grouped for clarity
        [Header("Detection Settings")]
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float playerDetectionInterval;
        [SerializeField] private float viewDistance;
        [SerializeField] private float viewAngle;
        [SerializeField] private float guaranteedDetectDistance;
        [SerializeField] private float suspicionIncreaseRate = 0.1f;
        [SerializeField] private SuspicionMeter suspicionMeter;

        private HealthComponent _healthComponent;
        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private Animator _animator;

        private List<InteractableObjects.IInteractable> _activeInteracts;
        private float _suspicionLevel = 0.0f;

        // state properties fields
        public Transform PlayerTransform { get; private set; }
        public StealthComponent PlayerStealthComponent { get; private set; }
        public bool IsPlayerInSight { get; private set; } = false;
        public bool IsOnAlert { get; set; } = false;
         
        public float SuspicionIncreaseRate => suspicionIncreaseRate;

        public float SuspicionLevel
        {
            get
            {
                return _suspicionLevel;
            }
            set
            {
                _suspicionLevel = Mathf.Clamp(value, 0.0f, 1.0f);

                suspicionMeter.SetVisibility(_suspicionLevel is < 1.0f and > 0.0f);
                suspicionMeter.SetBarFillAmount(_suspicionLevel);
            }
        }

        public override bool CanContact(IAILogic other)
        {
            var enemy = other as EnemyLogic;
            if (!enemy)
            {
                return false;
            }

            return (transform.position - enemy.transform.position).magnitude < viewDistance;
        }
        
        protected override void Start()
        {
            base.Start();
            InitializeComponents();
            StartCoroutine(PlayerDetectionCoroutine());
        }

        private void InitializeComponents()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateUpAxis = false;
            _agent.updateRotation = false;

            var player = ManagersOwner.GetManager<GameMode>().PlayerController;;
            PlayerTransform = player.transform;
            PlayerStealthComponent = player.GetComponent<StealthComponent>();
            _activeInteracts = new List<InteractableObjects.IInteractable>();

            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHandler;
        }

        protected override void Update()
        {
            base.Update();
            CheckForDoors();
            UpdateAnimator();
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

        private void UpdateAnimator()
        {
            _animator.SetBool(IsMoving, _agent.velocity != Vector3.zero);
        }

        private IEnumerator PlayerDetectionCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(playerDetectionInterval);
                IsPlayerInSight = CheckIfPlayerInSight();
            }
            // Don't need to stop the coroutine, it will be stopped when the object is destroyed
            // ReSharper disable once IteratorNeverReturns
        }

        private bool CheckIfPlayerInSight()
        {
            float distanceToPlayer = (transform.position - PlayerTransform.position).magnitude;
            if (distanceToPlayer > viewDistance)
            {
                return false;
            }

            Vector3 directionToPlayer = (PlayerTransform.position - transform.position).normalized;
            float playerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(Mathf.Abs(_rigidBody.rotation) - Mathf.Abs(playerAngle)) > 0.5f * viewAngle && viewDistance > guaranteedDetectDistance)
            {
                return false;
            }
            return !Physics2D.Linecast(transform.position, PlayerTransform.position, wallMask);
        }

        private void CheckForDoors()
        {
            foreach (var interactable in _activeInteracts)
            {
                var door = interactable as DoorBase;

                if (door)
                {
                    Vector3 directionToDoor = (door.transform.position - transform.position).normalized;
                    Vector3 velocity = _agent.velocity;
                    float dotProduct = directionToDoor.x * velocity.x + directionToDoor.y * velocity.y;
                    if (dotProduct > 0)
                    {
                        door.Interact(gameObject);
                    }
                }
            }
        }

        private void OnDeathHandler()
        {
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            var rb = GetComponent<Rigidbody2D>();
            var position = transform.position;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(position, guaranteedDetectDistance);

            const int iterations = 10;
            float originalAngle = rb.rotation;
            float stepAngles = viewAngle / iterations;
            float angle = originalAngle - 0.5f * viewAngle;

            Vector3 pos = position;
            for (var i = 0; i <= iterations; i++)
            {
                var rad = Mathf.Deg2Rad * angle;
                var secondPos = transform.position + new Vector3(viewDistance * Mathf.Cos(rad), viewDistance * Mathf.Sin(rad), 0);
                Gizmos.DrawLine(pos, secondPos);

                angle += stepAngles;
                pos = secondPos;
            }
            Gizmos.DrawLine(pos, transform.position);
        }
    }
}

