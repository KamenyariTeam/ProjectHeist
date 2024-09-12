using InteractableObjects.Weapons;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerWeaponComponent : WeaponComponent
    {
        [SerializeField] private float throwForce = 3f;
        [SerializeField] private float rotationForce = 0.5f;
        
        public void EquipWeapon(BaseWeapon weapon)
        {
            DropEquippedWeapon();
            equippedWeapon = weapon;
            SetupEquippedWeapon();
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
                Vector2 firePointPosition = firePoint.position;
                Vector2 throwDirection = MovementComponent.LookPosition - firePointPosition;

                rb.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);

                // Apply torque to add rotation to the weapon
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