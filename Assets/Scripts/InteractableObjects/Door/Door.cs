using System.Collections.Generic;
using Characters.AI;
using UnityEngine;

namespace InteractableObjects.Door
{
    public class Door : OutlinedInteractable
    {
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        [SerializeField] private float maxTimeOpened;

        [SerializeField] private List<Animator> doorAnimators;

        private bool _isOpened;
        private float _timeOpened;

        public override void Interact(GameObject interacter)
        {
            var isAI = interacter.GetComponent<IAILogic>() != null;
            if (!isAI || !_isOpened)
            {
                ChangeState();
            }
        }

        protected override void Start()
        {
            base.Start();
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