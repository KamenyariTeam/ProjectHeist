using UnityEngine;

namespace Characters.AI
{

    public interface IAIStateLogic
    {
        public void Init(IAILogic aiLogic);
        public void OnEnter();
        public void OnExit();
        public AIState OnUpdate(float deltaTime);
    }

    [System.Serializable]
    public class AIStateWrapper
    {
        [SerializeReference]
        public IAIStateLogic StateLogic;
    }

}
