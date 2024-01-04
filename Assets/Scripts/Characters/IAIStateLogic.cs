namespace Character
{
    enum AIState
    {
        Patrolling,
        Attacking,
        ChasingPlayer,
        SearchingForPlayer
    }

    interface IAIStateLogic
    {
        public void OnStart();
        public AIState OnUpdate(float timeDelta, bool isDetectingPlayer);
        public void OnStop();
    }
}
