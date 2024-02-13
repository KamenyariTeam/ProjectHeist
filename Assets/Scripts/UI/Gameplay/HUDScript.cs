using System;
using Characters.Player;
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
        private GameObject _player;

        private WeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        protected override void Awake()
        {
            base.Awake();
            _player = GameObject.FindGameObjectWithTag(playerTag);
            _weaponComponent = _player.GetComponent<WeaponComponent>();
            _healthComponent = _player.GetComponent<HealthComponent>();
        }

        private void Update()
        {
            ammoCount.SetText(_weaponComponent.CurrentAmmo + " / " + _weaponComponent.maxAmmo);
            healthCount.SetText(_healthComponent.CurrentHealth + " / " + _healthComponent.maxHealth);
            armorCount.SetText(_healthComponent.CurrentArmor + " / " + _healthComponent.maxArmor);
        }
    }
}