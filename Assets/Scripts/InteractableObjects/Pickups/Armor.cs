using Characters.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace InteractableObjects.Pickups
{
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
