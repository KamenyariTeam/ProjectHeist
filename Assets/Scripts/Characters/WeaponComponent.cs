using System.Collections;
using Characters.Player;
using DataStorage;
using UnityEngine;
using GameManagers;
using InteractableObjects.Weapons;

namespace Characters
{
    public class WeaponComponent : MonoBehaviour
    {
        public BaseWeapon equippedWeapon;
        public Transform firePoint;

        private bool _isAutoFiring;

        protected MovementComponent MovementComponent;
        private AudioManager _audioManager;
        private AnimationComponent _animationComponent;

        protected virtual void Start()
        {
            InitializeComponents();
            SetupEquippedWeapon();
        }
        
        private void Update()
        {
            if (_isAutoFiring)
            {
                HandleFire();
            }
        }

        private void InitializeComponents()
        {
            _audioManager = ManagersOwner.GetManager<AudioManager>();
            _animationComponent = GetComponent<AnimationComponent>();
            MovementComponent = GetComponent<MovementComponent>();
        }

        protected void SetupEquippedWeapon()
        {
            if (!equippedWeapon) return;

            firePoint.localPosition = equippedWeapon.firePointPosition;
            firePoint.localRotation = equippedWeapon.firePointRotation;
            equippedWeapon.BulletSpawnPoint = firePoint;
            
            equippedWeapon.transform.SetParent(transform);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
            
            equippedWeapon.GetComponent<Renderer>().enabled = false;
            equippedWeapon.GetComponent<Collider2D>().enabled = false;
            equippedWeapon.GetComponent<Rigidbody2D>().simulated = false;
        }

        public void StartFire()
        {
            if (!equippedWeapon) return;
            
            switch (equippedWeapon.fireMode)
            {
                case FireMode.SingleShot:
                    HandleFire();
                    break;
                case FireMode.SemiAuto:
                    HandleFire(); // Fires on each button press
                    break;
                case FireMode.FullAuto:
                    _isAutoFiring = true;
                    break;
            }
        }

        protected void StopFire()
        {
            _isAutoFiring = false;
        }

        private void HandleFire()
        {
            equippedWeapon?.Shoot(MovementComponent);
        }

        public void Reload()
        {
            if (!equippedWeapon) return;

            var weaponState = equippedWeapon.Reload();
            HandleWeaponState(weaponState);
        }

        public void ReloadAnimationEnded()
        {
            equippedWeapon?.ReloadEnded();
        }

        private void HandleWeaponState(WeaponStateData weaponState)
        {
            if (weaponState != null)
            {
                _animationComponent.PlayAnimation(weaponState.animation);
                _audioManager.PlaySound(weaponState.sound, transform, s => TableID.NONE);
            }
        }
    }
}
