using System.Collections.Generic;
using Characters.AI;
using UnityEngine;

namespace InteractableObjects.Door
{
    public class Door : OutlinedInteractable
    {
        [SerializeField] private float maxTimeOpened;

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _movementTime = 1.0f; // Duration of the slide
        private float _timer;
        private float _timeOpened = 0.0f;
        private bool _shouldSlide = false;
        private bool _isOpened = false;


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

            if (_shouldSlide)
            {
                if (_timer < _movementTime)
                {
                    _timer += Time.deltaTime;
                    transform.position = Vector3.Lerp(_startPosition, _endPosition, _timer / _movementTime);
                }
                else
                {
                    _shouldSlide = false;
                }
            }
        }

        private void ChangeState()
        {
            // Set position for the opposite direction
            float dir = _isOpened ? 1.0f : -1.0f;

            // TODO
            // Get correct renderer of movable door (Square, not Mirrage)
            float width = GetComponent<Renderer>().bounds.size.x;
            _startPosition = transform.position;
            _endPosition = _startPosition + dir * transform.right * width;

            _isOpened = !_isOpened;
            _timer = 0;
            _shouldSlide = true;
            _timeOpened = 0.0f;
        }
    }
}