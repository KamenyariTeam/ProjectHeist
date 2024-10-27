using UnityEngine;
using UnityEditor;

namespace Characters.AI
{

    public interface IAIStateLogic
    {
        public void OnEnter();
        public void OnExit();
        public AIState OnUpdate(float deltaTime);

        public void Init(IAILogic aiLogic);
    }

    [System.Serializable]
    public class AIStateWrapper
    {
        [SerializeReference]
        public IAIStateLogic StateLogic;
    }

}
