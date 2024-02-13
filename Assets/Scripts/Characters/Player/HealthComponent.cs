using SaveSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public delegate void FOnDeath();

    public class HealthComponent : MonoBehaviour, ISavableComponent
    {
        public float maxHealth = 100f;
        public float maxArmor = 100f;
        public float regenerationSpeed = 1f;
        public float regenerationRate = 1f;
        public FOnDeath OnDeath;

        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _currentArmor = 0f;

        private float _currentHealthRegenerationTime;

        public float CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = math.clamp(value, 0f, maxHealth);
                if (_currentHealth == 0f)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public float CurrentArmor
        {
            get => _currentArmor;
            private set
            {
                _currentArmor = math.clamp(value, 0f, maxArmor);
            }
        }

        private void Start()
        {

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

        public void TakeDamage(float Damage)
        {
            if (CurrentArmor > Damage)
            {
                CurrentArmor -= Damage;
            }
            else
            {
                Damage -= CurrentArmor;
                CurrentArmor = 0f;
                CurrentHealth -= Damage;
            }
        }

        public void TakeMedicine(float MedicineValue)
        {
            CurrentHealth += MedicineValue;
        }

        public void TakeArmor(float ArmorValue)
        {
            CurrentArmor += ArmorValue;
        }

        public ComponentData Serialize()
        {
            ComponentData data = new ComponentData();

            data.SetFloat("currentHealth", _currentHealth);
            data.SetFloat("currentArmor", _currentArmor);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            _currentHealth = data.GetFloat("currentHealth");
            _currentArmor = data.GetFloat("currentArmor");
        }
    }
}