using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Characters.Player;
using DataStorage.Generated;
using InteractableObjects.Pickups;
using SaveSystem;
using UnityEngine;

namespace Items.Weapons
{
    public class BaseWeapon : Pickup, ISavableComponent
    {
        public float damage = 1f; // Base damage of the weapon
        [SerializeField] protected float angleInaccuracy = 1.5f; // Angle inaccuracy for shooting, affecting bullet spread
        [SerializeField] protected float currentAmmo = 7f; // Current ammo count in the weapon
        [SerializeField] protected float maxAmmo = 7f; // Maximum ammo capacity of the weapon
        
        [SerializeField] protected float shotCooldown = 1f; // Cooldown time between shots
        
        public Vector2 firePointPosition;
        public Quaternion firePointRotation;
        [SerializeField] protected GameObject bulletPrefab; // Prefab for bullets spawned when shooting
        [SerializeField] protected GameObject flashPrefab; // Prefab for muzzle flash effects

        [SerializedDictionary("State", "Data")] 
        public SerializedDictionary<WeaponState, WeaponStateData> weaponStates = new()
        {
            {WeaponState.None, new WeaponStateData()},
        };
        
        private bool _isDelayedBetweenShots;
        private bool _isReloading;

        public bool CanAttack => !_isDelayedBetweenShots && !_isReloading;

        public float CurrentAmmo
        {
            get => currentAmmo;
            private set => currentAmmo = value < 0f ? 0f : value;
        }
        
        public float MaxAmmo => maxAmmo;

        public override void Interact(GameObject character)
        {
            PlayerWeaponComponent weaponComponent = character.GetComponent<PlayerWeaponComponent>();

            if (weaponComponent != null)
            {
                weaponComponent.EquipWeapon(this);
            }
        }
        
        public WeaponStateData Shoot(Transform bulletSpawnPoint, MovementComponent movementComponent)
        {
            if (!CanAttack)
            {
                return null;
            }

            if (CurrentAmmo > 0)
            {
                var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                Instantiate(flashPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

                var directionVector = GetShootingDirection(bulletSpawnPoint, movementComponent);

                bullet.GetComponent<BulletComponent>().direction = directionVector;

                --CurrentAmmo;
                StartCoroutine(ShotCooldownCoroutine());

                return GetWeaponState(WeaponState.Shoot);
            }
            
            return GetWeaponState(WeaponState.EmptyMagazine);
        }

        // Calculates the direction for the bullet to travel
        private Vector3 GetShootingDirection(Transform bulletSpawnPoint, MovementComponent movementComponent)
        {
            var directionVector = bulletSpawnPoint.right;
    
            if (!movementComponent.IsSneaking && movementComponent.MovementDirection != Vector2.zero)
            {
                var randomInaccuracy = UnityEngine.Random.Range(-angleInaccuracy, angleInaccuracy);
                directionVector = Quaternion.Euler(0, 0, randomInaccuracy) * directionVector;
            }

            return directionVector;
        }

        // Coroutine to handle the cooldown period between shots
        private IEnumerator ShotCooldownCoroutine()
        {
            _isDelayedBetweenShots = true;
            yield return new WaitForSeconds(shotCooldown);
            _isDelayedBetweenShots = false;
        }
        
        public WeaponStateData Reload()
        {
            _isReloading = true;
            return GetWeaponState(WeaponState.Reload);
        }

        // Ends the reload process and refills ammo
        public void ReloadEnded()
        {
            CurrentAmmo = MaxAmmo;
            _isReloading = false;
        }
        
        // Retrieves the weapon state data for a given state
        private WeaponStateData GetWeaponState(WeaponState state)
        {
            return weaponStates.ContainsKey(state) ? weaponStates[state] : weaponStates[WeaponState.None];
        }

        public ComponentData Serialize()
        {
            ExtendedComponentData data = new ExtendedComponentData();

            data.SetFloat("currentAmmo", currentAmmo);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            ExtendedComponentData unpacked = (ExtendedComponentData)data;

            currentAmmo = unpacked.GetFloat("currentAmmo");
        }
    }

    public enum WeaponState
    {
        None,
        PickUp,
        Shoot,
        Reload,
        EmptyMagazine
    }
    
    [Serializable]
    public class WeaponStateData
    {
        public AnimationClip animation;
        public SoundType sound;
    }
}