using UnityEngine;
using UnityEngine.AI;

namespace Characters.AI.Enemy
{
    [System.Serializable]
    public class SuspicionState : BaseAIStateLogic
    {

        [SerializeField] private AIState idleState = AIState.Patrolling;
        [SerializeField] private AIState alarmedState = AIState.Attacking;

        private NavMeshAgent _agent;
        private IAIPlayerDetectable _playerDetector;
        private Rigidbody2D _rigidBody;

        public override void Init(IAILogic aiLogic)
        {
            base.Init(aiLogic);
            _agent = _aiLogic.GetComponent<NavMeshAgent>();
            _rigidBody = _aiLogic.GetComponent<Rigidbody2D>();
            _playerDetector = aiLogic as IAIPlayerDetectable;
        }

        public override void OnEnter()
        {
            _agent.isStopped = true;
        }

        public override AIState OnUpdate(float deltaTime)
        {
            if (_playerDetector.IsPlayerInSight && _playerDetector.PlayerStealthComponent.IsNoticeable)
            {
                if (_playerDetector.IsOnAlert)
                {
                    return alarmedState;
                }
                
                _rigidBody.rotation = AIHelpers.RotateToObject(_aiLogic.transform.position, _playerDetector.PlayerTransform.position);

                _playerDetector.SuspicionLevel += deltaTime * _playerDetector.SuspicionIncreaseRate * _playerDetector.PlayerStealthComponent.Noticeability;

                // Transition to attack state if suspicion reaches 1
                if (_playerDetector.SuspicionLevel >= 1.0f)
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