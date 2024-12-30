namespace Characters.AI
{

    [System.Serializable]
    public abstract class BaseAIStateLogic: IAIStateLogic
    {
        protected IAILogic _aiLogic;

        public virtual void Init(IAILogic aiLogic)
        {
            _aiLogic = aiLogic;

        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual AIState OnUpdate(float deltaTime)
        {
            return _aiLogic.State;
        }
    }

}
