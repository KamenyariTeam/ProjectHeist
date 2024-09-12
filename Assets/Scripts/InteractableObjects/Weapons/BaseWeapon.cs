using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Characters.Player;
using DataStorage.Generated;
using InteractableObjects.Pickups;
using SaveSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InteractableObjects.Weapons
{
    public enum FireMode
    {
        SingleShot,
        SemiAuto,
        FullAuto
    }
    
    [RequireComponent(typeof(BulletSpawner), typeof(FlashSpawner))]
    public class BaseWeapon : Pickup, ISavableComponent
    {
        [SerializeField] protected float angleInaccuracy = 1.5f; // Angle inaccuracy for shooting, affecting bullet spread

        [SerializeField] protected int currentAmmo = 7; // Current ammo count in the weapon
        [SerializeField] protected int maxAmmo = 7; // Maximum ammo capacity of the weapon

        [SerializeField] protected float shotCooldown = 1f; // Cooldown time between shots
        [SerializeField] public FireMode fireMode = FireMode.SingleShot; // Fire mode of the weapon

        public Vector2 firePointPosition;
        public Quaternion firePointRotation;

        public Bullet bullet;
        public Flash flash;
        [NonSerialized] public Transform BulletSpawnPoint;

        [SerializedDictionary("State", "Data")]
        public SerializedDictionary<WeaponState, WeaponStateData> weaponStates = new()
        {
            { WeaponState.None, new WeaponStateData() }
        };

        private bool _isDelayedBetweenShots;
        private bool _isReloading;

        private BulletSpawner _bulletSpawner;
        private FlashSpawner _flashSpawner;

        public bool CanAttack => !_isDelayedBetweenShots && !_isReloading;

        public int CurrentAmmo
        {
            get => currentAmmo;
            private set
            {
                if (value < 0)
                {
                    Debug.LogWarning("Ammo count below zero; setting to 0.");
                    currentAmmo = 0;
                }
                else
                {
                    currentAmmo = value;
                }
            }
        }

        public int MaxAmmo => maxAmmo;

        protected override void Start()
        {
            base.Start();

            _bulletSpawner = GetComponent<BulletSpawner>();
            _flashSpawner = GetComponent<FlashSpawner>();
        }

        public override void Interact(GameObject character)
        {
            var weaponComponent = character.GetComponent<PlayerWeaponComponent>();

            if (weaponComponent != null) weaponComponent.EquipWeapon(this);
        }

        public WeaponStateData Shoot(MovementComponent movementComponent)
        {
            if (!CanAttack) return null;

            if (CurrentAmmo > 0)
            {
                SpawnBullet(movementComponent);
                SpawnMuzzleFlashEffect();

                --CurrentAmmo;
                StartCoroutine(ShotCooldownCoroutine());

                return GetWeaponState(WeaponState.Shoot);
            }

            return GetWeaponState(WeaponState.EmptyMagazine);
        }
        
        private void SpawnBullet(MovementComponent movementComponent)
        {
            Bullet bulletInstance = _bulletSpawner.Pool.Get();
            bulletInstance.SetDirection(GetShootingDirection(movementComponent));
        }

        private void SpawnMuzzleFlashEffect()
        {
            _flashSpawner.Pool.Get();
        }

        // Calculates the direction for the bullet to travel
        private Vector3 GetShootingDirection(MovementComponent movementComponent)
        {
            var directionVector = BulletSpawnPoint.right;

            if (!movementComponent.IsSneaking && movementComponent.MovementDirection != Vector2.zero)
            {
                var randomInaccuracy = Random.Range(-angleInaccuracy, angleInaccuracy);
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
            var data = new ExtendedComponentData();

            data.SetInt("currentAmmo", currentAmmo);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            var unpacked = (ExtendedComponentData)data;

            currentAmmo = unpacked.GetInt("currentAmmo");
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