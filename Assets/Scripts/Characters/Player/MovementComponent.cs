using UnityEngine;

namespace Characters.Player
{
    public class MovementComponent : MonoBehaviour
    {
        // Movement
        public float moveSpeed = 1f;
        private Rigidbody2D _rigidbody;
        private Vector2 _movementDirection;

        // Look and Aim
        private UnityEngine.Camera _camera;
        private Vector2 _lookPosition;
        public Vector2 LookPosition => _lookPosition;
        
        // Animation
        private AnimationComponent _animationComponent;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animationComponent = GetComponent<AnimationComponent>();
            _camera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (_camera)
            {
                UpdateLookPosition();
            }
        }

        private void FixedUpdate()
        {
            UpdateMovement();
            UpdateRotation();
        }

        private void UpdateLookPosition()
        {
            _lookPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void UpdateMovement()
        {
            _rigidbody.velocity = _movementDirection * moveSpeed;
            _animationComponent.UpdateMovementAnimation(_movementDirection != Vector2.zero);
        }

        private void UpdateRotation()
        {
            var lookDirection = _lookPosition - _rigidbody.position;
            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
        }

        public void HandleMove(Vector2 direction)
        {
            _movementDirection = direction;
        }
    }
}
