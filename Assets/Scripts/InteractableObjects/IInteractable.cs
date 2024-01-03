using UnityEngine;

namespace InteractableObjects
{
    public interface IInteractable
    {
        // TODO: replace GameObject with some Inventory class
        public void Interact(GameObject character);
    }
}