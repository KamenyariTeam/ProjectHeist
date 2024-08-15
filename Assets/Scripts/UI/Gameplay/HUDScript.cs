using System;
using Characters.Player;
using GameManagers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HUDScript : BaseUIWindow
    {
        [SerializeField] private string playerTag;
        [SerializeField] private TMP_Text ammoCount;
        [SerializeField] private TMP_Text healthCount;
        [SerializeField] private TMP_Text armorCount;

        private WeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        protected void Awake()
        {
            var player = ManagersOwner.GetManager<GameMode>().PlayerController;
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