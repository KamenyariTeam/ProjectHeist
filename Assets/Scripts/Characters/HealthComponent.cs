using InteractableObjects;
using SaveSystem;
using Unity.Mathematics;
using UnityEngine;

namespace Characters
{
    public delegate void FOnDeath();

    public class HealthComponent : MonoBehaviour, IDamageable, ISavableComponent
    {
        public float maxHealth = 100f;
        public float maxArmor = 100f;
        public float regenerationSpeed = 1f;
        public float regenerationRate = 1f;

        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float currentArmor;

        public event FOnDeath OnDeath;

        private float _currentHealthRegenerationTime;

        public float CurrentHealth
        {
            get => currentHealth;
            private set
            {
                currentHealth = math.clamp(value, 0f, maxHealth);
                if (currentHealth == 0f)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public float CurrentArmor
        {
            get => currentArmor;
            private set => currentArmor = math.clamp(value, 0f, maxArmor);
        }

        private void Update()
        {
            if (_currentHealthRegenerationTime > regenerationSpeed)
            {
                _currentHealthRegenerationTime = 0;
                CurrentHealth += regenerationRate;
            }
            else
            {
                _currentHealthRegenerationTime += Time.deltaTime;
            }
        }

        public void TakeDamage(float damage)
        {
            if (CurrentArmor > damage)
            {
                CurrentArmor -= damage;
            }
            else
            {
                damage -= CurrentArmor;
                CurrentArmor = 0f;
                CurrentHealth -= damage;
            }
        }

        public void TakeMedicine(float medicineValue)
        {
            CurrentHealth += medicineValue;
        }

        public void TakeArmor(float armorValue)
        {
            CurrentArmor += armorValue;
        }

        public ComponentData Serialize()
        {
            var data = new ComponentData();

            data.SetFloat("currentHealth", currentHealth);
            data.SetFloat("currentArmor", currentArmor);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            currentHealth = data.GetFloat("currentHealth");
            currentArmor = data.GetFloat("currentArmor");
        }
    }
}