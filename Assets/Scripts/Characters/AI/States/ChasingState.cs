using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI
{

    [System.Serializable]
    public class ChasingState : BaseAIStateLogic
    {
        public interface IStateProperties : IPlayerDetector
        {
        }

        private const float MaxTimeInState = 10.0f;

        [SerializeField] private AIState foundState = AIState.Attacking;
        [SerializeField] private AIState notFoundState = AIState.SearchingForPlayer;
        [SerializeField] private float speed = 1.4f;

        private NavMeshAgent _agent;
        private IStateProperties _properties;

        private float _timeInState;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _properties = aiLogic as IStateProperties;
        }

        public override void OnEnter()
        {
            _agent.SetDestination(_properties.PlayerTransform.position);
            _agent.isStopped = false;
            _agent.speed = speed;
            _timeInState = 0;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (_properties.IsPlayerInSight)
            {
                return foundState;
            }

            _timeInState += deltaTime;
            if (_timeInState >= MaxTimeInState ||
                (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance))
            {
                return notFoundState;
            }

            return _aiLogic.State;
        }
    }
}