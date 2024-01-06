using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField] private float attackContactDelay = 0.5f;

        private List<IAILogic> _characters;
        private float _attackContactTimer;

        private void Start()
        {
            _characters = new List<IAILogic>();
            foreach (var script in FindObjectsOfType<MonoBehaviour>())
            {
                IAILogic character = script as IAILogic;
                if (character != null)
                {
                    _characters.Add(character);
                }
            }
            _attackContactTimer = attackContactDelay;
        }

        private void Update()
        {
            _attackContactTimer -= Time.deltaTime;
            if (_attackContactTimer < 0.0f)
            {
                UpdateAttackingCharacters();
                _attackContactTimer = attackContactDelay;
            }

        }

        private void UpdateAttackingCharacters()
        {
            List<IAILogic> attackingCharacters = _characters.FindAll((character) => character.State == AIState.Attacking);
            List<IAILogic> nonAttackingCharacters = _characters.FindAll((character) => character.State != AIState.Attacking &&
                                                                                       character.HasState(AIState.Attacking));

            foreach (IAILogic nonAttacking in nonAttackingCharacters)
            {
                foreach (IAILogic attacking in attackingCharacters)
                {
                    if (attacking.CanContact(nonAttacking))
                    {
                        nonAttacking.State = AIState.Attacking;
                        break;
                    }
                }
            }
        }
    }
}
