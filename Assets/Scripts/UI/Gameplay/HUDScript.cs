using Characters.Player;
using GameManagers;
using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class HUDScript : BaseUIWindow
    {
        [SerializeField] private string playerTag;
        [SerializeField] private TMP_Text ammoCount;
        [SerializeField] private TMP_Text healthCount;
        [SerializeField] private TMP_Text armorCount;

        private PlayerWeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        protected void Awake()
        {
            var player = ManagersOwner.GetManager<GameMode>().PlayerController;
            _weaponComponent = player.GetComponent<PlayerWeaponComponent>();
            _healthComponent = player.GetComponent<HealthComponent>();
        }

        private void Update()
        {
            if (_weaponComponent.equippedWeapon)
            {
                ammoCount.enabled = true;
                ammoCount.SetText(_weaponComponent.equippedWeapon.CurrentAmmo + " / " +
                                  _weaponComponent.equippedWeapon.MaxAmmo);
            }
            else
            {
                ammoCount.enabled = false;
            }
            healthCount.SetText(_healthComponent.CurrentHealth + " / " + _healthComponent.maxHealth);
            armorCount.SetText(_healthComponent.CurrentArmor + " / " + _healthComponent.maxArmor);
        }
    }
}