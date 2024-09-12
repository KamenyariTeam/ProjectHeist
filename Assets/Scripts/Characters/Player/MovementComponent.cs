using UnityEngine;

namespace Characters.Player
{
    public class MovementComponent : MonoBehaviour
    {
        // Movement
        [SerializeField] private float moveSpeed = 1f;
        public bool IsSneaking { get; private set; }
        public Vector2 MovementDirection { get; private set; }
        private Rigidbody2D _rigidbody;

        // Look and Aim
        private UnityEngine.Camera _camera;
        private Transform _firePoint;
        private Vector2 _lookPosition;
        public Vector2 LookPosition => _lookPosition;
        
        // Animation
        private AnimationComponent _animationComponent;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animationComponent = GetComponent<AnimationComponent>();
            _camera = UnityEngine.Camera.main;
            
            var weaponComponent = GetComponent<PlayerWeaponComponent>();
            _firePoint = weaponComponent.firePoint;
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
            if (IsSneaking)
            {
                _rigidbody.velocity = MovementDirection * (moveSpeed * 0.5f);
            }
            else
            {
                _rigidbody.velocity = MovementDirection * moveSpeed;
            }
            
            _animationComponent.UpdateMovementAnimation(MovementDirection != Vector2.zero);
        }

        private void UpdateRotation()
        {
            Vector2 firePointPosition = _firePoint.position;
            var lookDirection = _lookPosition - firePointPosition;
            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
        }

        public void HandleMove(Vector2 direction)
        {
            MovementDirection = direction;
        }
        
        public void HandleSneak()
        {
            IsSneaking = !IsSneaking;
        }
    }
}
