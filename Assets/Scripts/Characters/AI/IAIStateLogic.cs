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
        public void OnStart();
        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer);
        public void OnStop();
    }

    public interface IAILogic
    {
        public AIState State { get; set; }
        public bool HasState(AIState state);
        public bool CanContact(IAILogic other);

    }
}
