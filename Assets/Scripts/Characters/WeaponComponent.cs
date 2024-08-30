using Characters.Player;
using UnityEngine;
using DataStorage.Generated;
using GameManagers;
using Items.Weapons;

namespace Characters
{
    public class WeaponComponent : MonoBehaviour
    {
        public BaseWeapon equippedWeapon;

        public Transform firePoint;

        protected AudioManager AudioManager;
        private AnimationComponent _animationComponent;

        protected virtual void Start()
        {
            AudioManager = ManagersOwner.GetManager<AudioManager>();
            _animationComponent = GetComponent<AnimationComponent>();

            if (equippedWeapon)
            {
                firePoint.localPosition = equippedWeapon.firePointPosition;
                firePoint.localRotation = equippedWeapon.firePointRotation;
            
                equippedWeapon.transform.SetParent(transform);
                equippedWeapon.transform.localPosition = Vector3.zero;
                equippedWeapon.transform.localRotation = Quaternion.identity;
            
                equippedWeapon.GetComponent<Renderer>().enabled = false;
                equippedWeapon.GetComponent<Collider2D>().enabled = false;
                equippedWeapon.GetComponent<Rigidbody2D>().simulated = false;
            }
        }

        public virtual void Shoot()
        {
            if (equippedWeapon && equippedWeapon.CanShoot)
            {
                if (equippedWeapon.CurrentAmmo > 0)
                {
                    var firePointPosition = firePoint.position;
                    var firePointRotation = firePoint.rotation;
                    var bullet = Instantiate(equippedWeapon.bulletPrefab, firePointPosition, firePointRotation);
                    var flash = Instantiate(equippedWeapon.flashPrefab, firePointPosition, firePointRotation);

                    var directionVector = firePoint.right;
                    var randomInaccuracy = Random.Range(-equippedWeapon.angleInaccuracy, equippedWeapon.angleInaccuracy);
                    directionVector = Quaternion.Euler(0, 0, randomInaccuracy) * directionVector;

                    bullet.GetComponent<BulletComponent>().direction = directionVector;

                    --equippedWeapon.CurrentAmmo;
                    AudioManager.PlaySound(equippedWeapon.shotSound, transform.position, s => SoundType.NONE);
                }
                else
                {
                    AudioManager.PlaySound(equippedWeapon.emptyShotSound, transform.position, s => SoundType.NONE);
                }
            }
        }

        public void Reload()
        {
            equippedWeapon.isReloading = true;
            AudioManager.PlaySound(equippedWeapon.reloadSound, transform, s => SoundType.NONE);

            _animationComponent.PlayReloadAnimation();
        }

        public void ReloadEnded()
        {
            equippedWeapon.CurrentAmmo = equippedWeapon.MaxAmmo;
            equippedWeapon.isReloading = false;
        }
    }
}