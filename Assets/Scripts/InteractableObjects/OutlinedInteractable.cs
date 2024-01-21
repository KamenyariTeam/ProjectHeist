using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractableObjects
{

    public abstract class OutlinedInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private Material outlineMaterial;

        private Material[] _originalMaterials;
        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value == _isSelected)
                {
                    return;
                }

                _isSelected = value;
                if (_isSelected)
                {
                    foreach (SpriteRenderer renderer in renderers)
                    {
                        renderer.material = outlineMaterial;
                    }
                }
                else
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].material = _originalMaterials[i];
                    }
                }
            }
        }

        public abstract void Interact(GameObject character);

        protected virtual void Start()
        {
            _originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _originalMaterials[i] = renderers[i].material;
            }
            _isSelected = false;
        }

    }
}
