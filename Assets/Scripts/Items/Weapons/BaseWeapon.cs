using Characters;
using Characters.Player;
using DataStorage.Generated;
using InteractableObjects;
using InteractableObjects.Pickups;
using SaveSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items.Weapons
{
    public class BaseWeapon : Pickup, ISavableComponent
    {
        public float damage = 1f;
        public float angleInaccuracy = 1.5f;
        [SerializeField] protected float currentAmmo = 7f;
        [SerializeField] protected float maxAmmo = 7f;
        
        public float shotCooldown = 1f;
        private float _timeSinceLastShot;
        
        public Vector2 firePointPosition;
        public Quaternion firePointRotation;
        public GameObject bulletPrefab;
        public GameObject flashPrefab;

        public SoundType shotSound;
        public SoundType emptyShotSound;
        public SoundType reloadSound;
        
        public bool isDelayedBetweenShots = false;
        public bool isReloading = false;

        public bool CanShoot => !isDelayedBetweenShots && !isReloading;

        public float CurrentAmmo
        {
            get => currentAmmo;
            set => currentAmmo = value < 0f ? 0f : value;
        }
        
        public float MaxAmmo
        {
            get => maxAmmo;
            private set => maxAmmo = value < 0f ? 0f : value;
        }
        
        public override void Interact(GameObject character)
        {
            PlayerWeaponComponent weaponComponent = character.GetComponent<PlayerWeaponComponent>();

            if (weaponComponent != null)
            {
                weaponComponent.EquipWeapon(this);
            }
        }

        private void Update()
        {
            if (isDelayedBetweenShots)
            {
                if (_timeSinceLastShot < shotCooldown)
                {
                    _timeSinceLastShot += Time.deltaTime;
                }
                else
                {
                    _timeSinceLastShot = 0;
                    isDelayedBetweenShots = false;
                }
            }
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
    
    struct WeaponStateData
    {
        public AnimationClip Animation;
        public SoundType Sound;
    }
}