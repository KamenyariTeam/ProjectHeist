using UnityEngine;

namespace InteractableObjects.Pickups
{
    public class Pickup : OutlinedInteractable
    {
        public override void Interact(GameObject character)
        {
            Destroy(gameObject);
        }
    }
}
