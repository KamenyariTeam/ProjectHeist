using UnityEngine;

namespace Characters.AI
{

    public abstract class IAILogic : MonoBehaviour
    {
        public abstract AIState State { get; set; }
        public abstract bool HasState(AIState state);
        public abstract bool CanContact(IAILogic other);
    }

}