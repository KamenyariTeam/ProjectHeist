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
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    UpdateMaterials();
                }
            }
        }

        public abstract void Interact(GameObject character);

        protected virtual void Start()
        {
            CacheOriginalMaterials();
        }

        private void CacheOriginalMaterials()
        {
            _originalMaterials = new Material[renderers.Length];
            for (var i = 0; i < renderers.Length; i++)
            {
                _originalMaterials[i] = renderers[i].material;
            }
        }

        private void UpdateMaterials()
        {
            if (_isSelected)
            {
                ApplyOutlineMaterial();
            }
            else
            {
                RestoreOriginalMaterials();
            }
        }

        private void ApplyOutlineMaterial()
        {
            foreach (var spriteRender in renderers)
            {
                spriteRender.material = outlineMaterial;
            }
        }

        private void RestoreOriginalMaterials()
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = _originalMaterials[i];
            }
        }
    }
}