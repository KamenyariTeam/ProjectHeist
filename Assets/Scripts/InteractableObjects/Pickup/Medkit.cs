using Character;
using SuperTiled2Unity.Editor.ClipperLib;
using UnityEngine;

namespace InteractableObjects
{
    public class MedKit : Pickup
    {
        public float HealthRegeneration = 25f;

        public override void Interact(GameObject character)
        {
            base.Interact(character);

            HealthComponent healthComponent = character.GetComponent<HealthComponent>();

            if (healthComponent != null)
            {
                healthComponent.TakeMedicine(HealthRegeneration);
            }
        }
    }

}
