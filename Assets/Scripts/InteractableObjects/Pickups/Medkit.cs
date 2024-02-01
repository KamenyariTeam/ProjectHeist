using Characters.Player;
using UnityEngine;

namespace InteractableObjects.Pickups
{
    public class MedKit : Pickup
    {
        public float healthRegeneration = 25f;

        public override void Interact(GameObject character)
        {
            base.Interact(character);

            HealthComponent healthComponent = character.GetComponent<HealthComponent>();

            if (healthComponent != null)
            {
                healthComponent.TakeMedicine(healthRegeneration);
            }
        }
    }

}
