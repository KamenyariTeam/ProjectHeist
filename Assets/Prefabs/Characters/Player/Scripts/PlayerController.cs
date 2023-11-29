using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1f;
        public float collisionOffset = 0.05f;
        public ContactFilter2D movementFilter;

        private Rigidbody2D _rigidbody;
        private PlayerInput _playerInput;
        private Vector2 _movementInput;
        private Vector2 _lookInput;
        private readonly List<RaycastHit2D> _castCollisions = new();

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
            if (_movementInput != Vector2.zero)
            {
                var success = TryMove(_movementInput);

                if (!success)
                    success = TryMove(new Vector2(_movementInput.x, 0));

                if (!success)
                    success = TryMove(new Vector2(0, _movementInput.y));

                _animator.SetBool(IsMoving, success);
            }
            else
            {
                _animator.SetBool(IsMoving, false);
            }

            var lookDirection = _lookInput - _rigidbody.position;
            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
        }

        private bool TryMove(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                var count = _rigidbody.Cast(
                    direction, // Represents direction (values between -1 and 1)
                    movementFilter, // The setting that determine where a collision can occur on such as layer to collide with
                    _castCollisions, // List of collisions to store the found collisions into after the Cast is finished
                    moveSpeed * Time.fixedDeltaTime * collisionOffset);

                if (count == 0)
                {
                    _rigidbody.MovePosition(_rigidbody.position + direction * (moveSpeed * Time.fixedDeltaTime));
                    return true;
                }
            }

            return false;
        }

        private void OnMove(InputValue movementValue)
        {
            _movementInput = movementValue.Get<Vector2>();
        }
    }
}
