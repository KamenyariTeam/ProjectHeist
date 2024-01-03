using InteractableObjects.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character
{
    public class WeaponComponent : MonoBehaviour
    {
        public float damage = 1f;
        public float maxAmmo = 7f;
        public float angleInaccuracy = 1.5f;
        public float shotCooldown = 1f;

        public Transform firePoint;
        public GameObject bulletPrefab;
        public GameObject flashPrefab;

        private float _currentAmmo = 7f;
        private float _timeSinceLastShot;

        private void Start()
        {
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

        public void Shoot()
        {
            if (CanShoot && _currentAmmo > 0)
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
            }
        }

        public void Reload()
        {
            CanShoot = false;
            _currentAmmo = maxAmmo;
        }

        public void ReloadEnded()
        {
            CanShoot = true;
        }
    }
}