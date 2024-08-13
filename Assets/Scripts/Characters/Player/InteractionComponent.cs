using System.Collections.Generic;
using InteractableObjects;
using UnityEngine;

namespace Characters.Player
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private GameObject currentTool;
        private readonly List<IInteractable> _activeInteracts = new();
        private MovementComponent _movementComponent;

        private void Start()
        {
            _movementComponent = GetComponent<MovementComponent>();
        }

        private void FixedUpdate()
        {
            UpdateSelectedInteractable();
        }

        private void OnTriggerEnter2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _activeInteracts.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _activeInteracts.Remove(interactable);
                interactable.IsSelected = false;
                UpdateSelectedInteractable();
            }
        }

        public void HandleInteract()
        {
            IInteractable selected = _activeInteracts.Find(interactable => interactable.IsSelected);
            selected?.Interact(gameObject);
        }

        public void HandleUse()
        {
            if (currentTool)
            {
                var tool = currentTool.GetComponent<Tools.ITool>();
                tool?.UseTool(gameObject);
            }
        }

        private void UpdateSelectedInteractable()
        {
            foreach (IInteractable interactable in _activeInteracts)
            {
                interactable.IsSelected = false;
            }

            var selectedInteractable = GetCurrentInteractable();
            if (selectedInteractable != null)
            {
                selectedInteractable.IsSelected = true;
            }
        }

        private IInteractable GetCurrentInteractable()
        {
            IInteractable closestInteractable = null;
            float closestDist = float.MaxValue;

            foreach (IInteractable interactable in _activeInteracts)
            {
                if (interactable is MonoBehaviour component)
                {
                    if (IsComponentVisible(component))
                    {
                        float dist = Vector2.Distance(_movementComponent.LookPosition, component.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestInteractable = interactable;
                        }
                    }
                }
            }

            return closestInteractable;
        }

        private bool IsComponentVisible(MonoBehaviour component)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, component.transform.position, wallLayer);
            if (!hit)
            {
                return true;
            }

            GameObject hitObject = hit.collider.gameObject;
            GameObject interactableObject = component.gameObject;

            return hitObject.GetInstanceID() == interactableObject.GetInstanceID()
                   || hitObject.transform.IsChildOf(interactableObject.transform);
        }
    }
}
