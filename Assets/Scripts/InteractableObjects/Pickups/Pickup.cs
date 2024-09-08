using Characters;
using Characters.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace InteractableObjects.Pickups
{
    public class Pickup : OutlinedInteractable
    {
        public override void Interact(GameObject character)
        {
            Destroy(gameObject);
        }
    }

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

    public class Armor : Pickup
    {
        [FormerlySerializedAs("ArmorRegeneration")] public float armorRegeneration = 25f;

        public override void Interact(GameObject character)
        {
            base.Interact(character);

            HealthComponent healthComponent = character.GetComponent<HealthComponent>();

            if (healthComponent != null)
            {
                healthComponent.TakeArmor(armorRegeneration);
            }
        }
    }
}
