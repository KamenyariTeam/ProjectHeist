using SaveSystem;
using UnityEngine;
using GameControllers.Audio;
using GameControllers;
using DataStorage.Generated;

namespace Characters.Player
{
    public class WeaponComponent : MonoBehaviour, ISavableComponent
    {
        public float damage = 1f;
        public float maxAmmo = 7f;
        public float angleInaccuracy = 1.5f;
        public float shotCooldown = 1f;

        public Transform firePoint;
        public GameObject bulletPrefab;
        public GameObject flashPrefab;

        [SerializeField] private SoundType _shotSound;
        [SerializeField] private SoundType _emptyShotSound;
        [SerializeField] private SoundType _reloadSound;

        [SerializeField] private float _currentAmmo = 7f;
        private float _timeSinceLastShot;

        private AudioManager _audioManager;
        private AnimationComponent _animationComponent;

        private void Start()
        {
            _audioManager = ManagersOwner.GetManager<AudioManager>();
            _animationComponent = GetComponent<AnimationComponent>();
            
            _currentAmmo = maxAmmo;
            CanShoot = true;
        }

        private void Update()
        {
            if (!CanShoot)
            {
                if (_timeSinceLastShot < shotCooldown)
                {
                    _timeSinceLastShot += Time.deltaTime;
                }
                else
                {
                    _timeSinceLastShot = 0f;
                    CanShoot = true;
                }
            }
        }

        public float CurrentAmmo
        {
            get => _currentAmmo;
            private set => _currentAmmo = value < 0f ? 0f : value;
        }

        public bool CanShoot { get; private set; }
        
        public void HandleFire()
        {
            Shoot();
        }
        
        public void HandleReload()
        {
            Reload();
        }

        public void Shoot()
        {
            if (!CanShoot)
            {
                return;
            }

            if (_currentAmmo > 0)
            {
                var firePointPosition = firePoint.position;
                var firePointRotation = firePoint.rotation;
                var bullet = Instantiate(bulletPrefab, firePointPosition, firePointRotation);
                var flash = Instantiate(flashPrefab, firePointPosition, firePointRotation);

                var randomInaccuracy = Random.Range(-angleInaccuracy, angleInaccuracy);
                var directionVector = firePoint.right;
                directionVector = Quaternion.Euler(0, 0, randomInaccuracy) * directionVector;

                bullet.GetComponent<BulletComponent>().direction = directionVector;

                --_currentAmmo;
                CanShoot = false;
                _audioManager.PlaySound(_shotSound, transform.position, s => SoundType.NONE);
            }
            else
            {
                _audioManager.PlaySound(_emptyShotSound, transform.position, s => SoundType.NONE);
            }

        }

        public void Reload()
        {
            CanShoot = false;
            _currentAmmo = maxAmmo;
            _audioManager.PlaySound(_reloadSound, transform, s => SoundType.NONE);

            _animationComponent.PlayReloadAnimation();
        }

        public void ReloadEnded()
        {
            CanShoot = true;
        }

        public ComponentData Serialize()
        {
            ExtendedComponentData data = new ExtendedComponentData();

            data.SetFloat("currentAmmo", _currentAmmo);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            ExtendedComponentData unpacked = (ExtendedComponentData)data;

            _currentAmmo = unpacked.GetFloat("currentAmmo");
        }
    }
}