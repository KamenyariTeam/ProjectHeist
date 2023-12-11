using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1f;

        private Rigidbody2D _rigidbody;
        private PlayerInput _playerInput;
        private Vector2 _movementInput;
        private Vector2 _lookInput;

        private Animator _animator;
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _playerInput = GetComponent<PlayerInput>();

            if (_playerInput.camera == null)
                Debug.LogWarning("Set reference for camera in PlayerInput on the current scene");
        }

        private void Update()
        {
            if (_playerInput.camera)
                _lookInput = _playerInput.camera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = _movementInput * moveSpeed;
            _animator.SetBool(IsMoving, _movementInput != Vector2.zero);

            var lookDirection = _lookInput - _rigidbody.position;
            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
        }

        private void OnMove(InputValue movementValue)
        {
            _movementInput = movementValue.Get<Vector2>();
        }
    }
}
