using Characters.AI.Enemy;

namespace Characters.AI
{
    public enum AIState
    {
        Patrolling,
        Attacking,
        ChasingPlayer,
        SearchingForPlayer
    }

    public interface IAIStateLogic
    {
        public void Init(EnemyLogic enemyLogic);
        public void OnEnter();
        public void OnExit();
        public AIState OnUpdate();
    }

    public interface IAILogic
    {
        public AIState State { get; set; }
        public bool HasState(AIState state);
        public bool CanContact(IAILogic other);

    }
}
