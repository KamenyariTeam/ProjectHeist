using UnityEngine;

namespace InteractableObjects
{
    public class Pickup : OutlinedInteractable
    {
        public override void Interact(GameObject character)
        {
            Destroy(gameObject);
        }
    }
}
