using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    public class EnemyLogic : MonoBehaviour, ICharacter
    {
        private static readonly float MinDistanceToPathPointMul = 2;
        private static readonly float MinDistanceToPathPointAdd = 0.2f;

        [SerializeField] private Transform[] initialPath;

        private NavMeshAgent _agent;

        private Rigidbody2D _rigidbody;

        private int _pathIndex;

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
            _pathIndex = 0;
            _agent.SetDestination(initialPath[_pathIndex].position);

        }

        // Update is called once per frame
        private void Update()
        {

            if (_agent.velocity != Vector3.zero)
            {
                var angle = Mathf.Atan2(_agent.velocity.y, _agent.velocity.x) * Mathf.Rad2Deg;
                _rigidbody.rotation = angle;
            }

            float distanceToPathPoint = (transform.position - initialPath[_pathIndex].position).magnitude;
            if (distanceToPathPoint < _agent.stoppingDistance * MinDistanceToPathPointMul + MinDistanceToPathPointAdd)
            {
                _pathIndex = (_pathIndex + 1) % initialPath.Length;
                _agent.SetDestination(initialPath[_pathIndex].position);
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

    }
}

