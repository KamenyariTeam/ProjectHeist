using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI
{
    [System.Serializable]
    public class SuspicionState : BaseAIStateLogic
    {
        public interface IStateProperties : IPlayerDetector
        {
        }

        [SerializeField] private AIState idleState = AIState.Patrolling;
        [SerializeField] private AIState alarmedState = AIState.Attacking;

        private NavMeshAgent _agent;
        private Rigidbody2D _rigidBody;
        private IStateProperties _properties;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _properties = aiLogic as IStateProperties;
        }

        public override void OnEnter()
        {
            _agent.isStopped = true;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (_properties.IsPlayerInSight && _properties.PlayerStealthComponent.IsNoticeable)
            {
                if (_properties.IsOnAlert)
                {
                    return alarmedState;
                }
                
                _rigidBody.rotation = AIHelpers.RotateToObject(_aiLogic.transform.position, _properties.PlayerTransform.position);
                _properties.SuspicionLevel += deltaTime * _properties.SuspicionIncreaseRate * _properties.PlayerStealthComponent.Noticeability;

                // Transition to attack state if suspicion reaches 1
                if (_properties.SuspicionLevel >= 1.0f)
                {
                    return alarmedState;
                }
            }
            else
            {
                return idleState;
            }
            
            return _aiLogic.State;
        }
    }
}