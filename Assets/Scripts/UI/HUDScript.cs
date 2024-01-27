using Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDScript : MonoBehaviour
    {
        public GameObject player;
        public TMP_Text ammoCount;
        public TMP_Text healthCount;
        public TMP_Text armorCount;

        private WeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        private void Start()
        {
            _weaponComponent = player.GetComponent<WeaponComponent>();
            _healthComponent = player.GetComponent<HealthComponent>();
        }

        private void Update()
        {
            ammoCount.SetText(_weaponComponent.CurrentAmmo + " / " + _weaponComponent.maxAmmo);
            healthCount.SetText(_healthComponent.CurrentHealth + " / " + _healthComponent.maxHealth);
            armorCount.SetText(_healthComponent.CurrentArmor + " / " + _healthComponent.maxArmor);
        }
    }
}