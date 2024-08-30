using DataStorage.Generated;
using Items.Weapons;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerWeaponComponent : WeaponComponent
    {
        private MovementComponent _movementComponent;

        protected override void Start()
        {
            base.Start();
            
            _movementComponent = GetComponent<MovementComponent>();
        }

        public override void Shoot()
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
                    if (!_movementComponent.IsSneaking && _movementComponent.MovementDirection != Vector2.zero)
                    {
                        var randomInaccuracy =
                            Random.Range(-equippedWeapon.angleInaccuracy, equippedWeapon.angleInaccuracy);
                        directionVector = Quaternion.Euler(0, 0, randomInaccuracy) * directionVector;
                    }

                    bullet.GetComponent<BulletComponent>().direction = directionVector;

                    --equippedWeapon.CurrentAmmo;
                    equippedWeapon.isDelayedBetweenShots = true;
                    AudioManager.PlaySound(equippedWeapon.shotSound, transform.position, s => SoundType.NONE);
                }
                else
                {
                    AudioManager.PlaySound(equippedWeapon.emptyShotSound, transform.position, s => SoundType.NONE);
                }
            }
        }
        
        public void EquipWeapon(BaseWeapon weapon)
        {
            DropEquippedWeapon();
            equippedWeapon = weapon;
            firePoint.localPosition = equippedWeapon.firePointPosition;
            firePoint.localRotation = equippedWeapon.firePointRotation;
            
            equippedWeapon.transform.SetParent(transform);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
            
            equippedWeapon.GetComponent<Renderer>().enabled = false;
            equippedWeapon.GetComponent<Collider2D>().enabled = false;
            equippedWeapon.GetComponent<Rigidbody2D>().simulated = false;
        }

        private void DropEquippedWeapon()
        {
            if (equippedWeapon == null) return;

            // Enable the weapon's renderer and collider to make it visible and interactable
            equippedWeapon.GetComponent<Renderer>().enabled = true;
            equippedWeapon.GetComponent<Collider2D>().enabled = true;
            equippedWeapon.GetComponent<Rigidbody2D>().simulated = true;

            // Unparent the weapon so it can move independently
            equippedWeapon.transform.SetParent(null);

            // Apply force to throw the weapon in the specified direction
            var rb = equippedWeapon.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float throwForce = 3f; // Adjust throw force as needed
                
                Vector2 firePointPosition = firePoint.position;
                Vector2 throwDirection = _movementComponent.LookPosition - firePointPosition;

                rb.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);

                // Apply torque to add rotation to the weapon
                float rotationForce = 1f; // Adjust rotation force as needed
                rb.AddTorque(rotationForce, ForceMode2D.Impulse);
            }

            // Nullify the reference to the equipped weapon since it's no longer held
            equippedWeapon = null;
        }
        
        public void HandleFire()
        {
            Shoot();
        }
        
        public void HandleReload()
        {
            Reload();
        }
        
        public void HandleThrowWeapon()
        {
            DropEquippedWeapon();
        }
    }
}