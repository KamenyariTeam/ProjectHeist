using Characters.AI.Enemy;

namespace Characters.AI
{
    public enum AIState
    {
        Patrolling,
        Attacking,
        ChasingPlayer,
        SearchingForPlayer,
        Suspicion
    }

    public interface IAIStateLogic
    {
        public void Init(IAILogic aiLogic);
        public void OnEnter();
        public void OnExit();
        public AIState OnUpdate(float deltaTime);
    }

    public interface IAILogic
    {
        public AIState State { get; set; }
        public bool HasState(AIState state);
        public bool CanContact(IAILogic other);

    }
}
