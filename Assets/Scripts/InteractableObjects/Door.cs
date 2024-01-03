using System.Collections.Generic;
using UnityEngine;

namespace InteractableObjects
{
    public class Door : MonoBehaviour
    {
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        [SerializeField] private Interactable interactableArea;

        [SerializeField] private float maxTimeOpened;

        [SerializeField] private List<Animator> doorAnimators;

        private bool _isOpened;
        private float _timeOpened;

        private void Start()
        {
            interactableArea.OnInteractEvent += ChangeState;

            _isOpened = false;
            _timeOpened = 0.0f;
        }

        private void Update()
        {
            if (_isOpened)
            {
                _timeOpened += Time.deltaTime;
                if (_timeOpened > maxTimeOpened) ChangeState();
            }
        }

        private void OnDestroy()
        {
            interactableArea.OnInteractEvent -= ChangeState;
        }

        private void ChangeState()
        {
            _isOpened = !_isOpened;
            foreach (var animator in doorAnimators) animator.SetBool(IsOpen, _isOpened);
            _timeOpened = 0.0f;
        }
    }
}