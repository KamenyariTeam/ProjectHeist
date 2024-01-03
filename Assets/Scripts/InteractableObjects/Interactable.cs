using UnityEngine;

namespace InteractableObjects
{
    public class Interactable : MonoBehaviour
    {
        public delegate void OnInteract();

        public event OnInteract OnInteractEvent;

        public void Interact()
        {
            OnInteractEvent?.Invoke();
        }
    }
}