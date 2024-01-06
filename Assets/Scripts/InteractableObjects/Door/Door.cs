using System.Collections.Generic;
using UnityEngine;

namespace InteractableObjects
{
    public class Door : MonoBehaviour, IInteractable
    {
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        [SerializeField] private float maxTimeOpened;

        [SerializeField] private List<Animator> doorAnimators;

        private bool _isOpened;
        private float _timeOpened;

        public void Interact(GameObject interacter)
        {
            bool isAI = interacter.GetComponent<Character.IAILogic>() != null;
            if (!isAI || !_isOpened)
            {
                ChangeState();
            }
        }

        private void Start()
        {
            _isOpened = false;
            _timeOpened = 0.0f;
        }

        private void Update()
        {
            if (_isOpened)
            {
                _timeOpened += Time.deltaTime;
                if (_timeOpened > maxTimeOpened)
                {
                    ChangeState();
                }
            }
        }


        private void ChangeState()
        {
            _isOpened = !_isOpened;
            foreach (var animator in doorAnimators)
            {
                animator.SetBool(IsOpen, _isOpened);
            }
            _timeOpened = 0.0f;
        }
    }
}