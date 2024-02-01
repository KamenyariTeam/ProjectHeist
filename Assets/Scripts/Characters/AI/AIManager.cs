using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.AI
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField] private float attackContactDelay = 0.5f;

        private List<IAILogic> _characters;
        private float _attackContactTimer;

        private void Start()
        {
            InitializeCharacters();
            ResetTimer();
        }

        private void InitializeCharacters()
        {
            _characters = new List<IAILogic>(FindObjectsOfType<MonoBehaviour>().OfType<IAILogic>());
        }

        private void Update()
        {
            if (IsTimerElapsed())
            {
                UpdateAttackingCharacters();
                ResetTimer();
            }
        }

        private bool IsTimerElapsed()
        {
            _attackContactTimer -= Time.deltaTime;
            return _attackContactTimer < 0.0f;
        }

        private void ResetTimer()
        {
            _attackContactTimer = attackContactDelay;
        }

        private void UpdateAttackingCharacters()
        {
            foreach (IAILogic character in _characters)
            {
                if (character.State != AIState.Attacking && character.HasState(AIState.Attacking))
                {
                    if (IsAnyAttackingCharacterInContact(character))
                    {
                        character.State = AIState.Attacking;
                    }
                }
            }
        }

        private bool IsAnyAttackingCharacterInContact(IAILogic character)
        {
            foreach (IAILogic attackingCharacter in _characters)
            {
                if (attackingCharacter.State == AIState.Attacking && attackingCharacter.CanContact(character))
                {
                    return true;
                }
            }
            return false;
        }
    }
}