namespace Character
{
    enum AIState
    {
        Idle,
        Attacking,
        OnAlarm,
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
