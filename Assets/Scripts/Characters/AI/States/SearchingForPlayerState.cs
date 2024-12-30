using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI
{
    [System.Serializable]
    public class SearchingForPlayerState : BaseAIStateLogic
    {
        public interface IStateProperties : IPlayerDetector
        {
        }

        [SerializeField] private AIState foundState = AIState.Attacking;
        [SerializeField] private AIState notFoundState = AIState.Patrolling;
        [SerializeField] private float searchingRotationSpeed = 45.0f;
        [SerializeField] private float totalRotationAngle = 360.0f;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private IStateProperties _properties;

        private float _totalRotation;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _properties = aiLogic as IStateProperties;
        }

        public override void OnEnter()
        {
            _totalRotation = 0;
            _agent.isStopped = true;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (_properties.IsPlayerInSight)
            {
                return foundState;
            }
            float angleDelta = searchingRotationSpeed * deltaTime;
            if (_totalRotation < totalRotationAngle)
            {
                _rigidBody.rotation += angleDelta;
                if (_rigidBody.rotation > 180.0f)
                    _rigidBody.rotation -= 360.0f;
            }
            else
            {
                _rigidBody.rotation -= angleDelta;
                if (_rigidBody.rotation < -180.0f) 
                    _rigidBody.rotation += 360.0f;
            }

            _totalRotation += angleDelta;
            return _totalRotation > 3.0f * totalRotationAngle ? notFoundState : _aiLogic.State;
        }
    }
}