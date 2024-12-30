using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Characters.AI
{
    public class BaseAILogic : IAILogic
    {
        [SerializeField] protected SerializedDictionary<AIState, AIStateWrapper> statesLogic;
        [SerializeField] protected AIState initialState;

        protected AIState _state;

        public override AIState State
        {
            get => _state;
            set
            {
                if (value == _state)
                {
                    return;
                }
 
                statesLogic[_state].StateLogic.OnExit();
                _state = value;
                statesLogic[_state].StateLogic.OnEnter();

                Debug.Log($"Entered state: {_state}");
            }
        }

        public override bool HasState(AIState state)
        {
            return statesLogic.ContainsKey(state);
        }

        public override bool CanContact(IAILogic other)
        {
            return false;
        }

        protected virtual void Start()
        {
            foreach (var stateLogic in statesLogic.Values)
            {
                stateLogic.StateLogic.Init(this);
            }
            statesLogic[_state].StateLogic.OnEnter();
        }

        protected virtual void Update()
        {
            AIState newState = statesLogic[_state].StateLogic.OnUpdate(Time.deltaTime);
            State = newState;
        }

    }
}

