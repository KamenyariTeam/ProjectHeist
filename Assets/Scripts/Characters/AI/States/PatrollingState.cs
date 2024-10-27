using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI
{
    [System.Serializable]
    public class PatrollingState : BaseAIStateLogic
    {
        public interface IStateProperties : IPlayerDetector
        {
        }

        [SerializeField] private AIState alarmedState = AIState.Attacking;
        [SerializeField] private AIState suspicionState = AIState.Suspicion;
        [SerializeField] private Transform[] path;
        [SerializeField] private float speed = 1.2f;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private IStateProperties _properties;

        private int _pathIndex;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _properties = aiLogic as IStateProperties;
        }

        public override void OnEnter()
        {
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.SetDestination(path[_pathIndex].position);
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (_properties.IsPlayerInSight && _properties.PlayerStealthComponent.IsNoticeable)
            {
                return _properties.IsOnAlert ? alarmedState : suspicionState;
            }

            if (!_properties.IsOnAlert)
            {
                _properties.SuspicionLevel -= deltaTime * _properties.SuspicionIncreaseRate;
            }
            
            MoveAlongPath();

            return _aiLogic.State;
        }

        public override void OnExit()
        {
        }

        private void MoveAlongPath()
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _pathIndex = (_pathIndex + 1) % path.Length;
                _agent.SetDestination(path[_pathIndex].position);
            }
            else
            {
                var velocity = _agent.velocity;
                var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                _rigidBody.rotation = angle;
            }
        }

    }
}