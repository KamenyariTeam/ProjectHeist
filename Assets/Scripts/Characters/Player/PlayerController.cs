using System.Collections.Generic;
using InteractableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        public float moveSpeed = 1f;

        // Pick up subsystem
        public BoxCollider2D interactBoxCollider;

        // Shooting
        public WeaponComponent currentWeapon;

        private Rigidbody2D _rigidbody;
        private UnityEngine.Camera _camera;
        private InputReader _input;
        private Vector2 _movementDirection;
        private readonly List<IInteractable> _activeInteracts = new();

        // Animation
        private Animator _animator;
        
        public Vector2 LookPosition { get; private set; }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _camera = UnityEngine.Camera.main;
            
            _input = ScriptableObject.CreateInstance<InputReader>();
            
            // Setup inputs
            _input.MoveEvent += HandleMove;
            _input.LookMouseEvent += HandleLookMouse;
            _input.LookGamepadEvent += HandleLookGamepad;
            _input.FireEvent += HandleFire;
            _input.ReloadEvent += HandleReload;
            _input.InteractEvent += HandleInteract;
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = _movementDirection * moveSpeed;
            _animator.SetBool(IsMoving, _movementDirection != Vector2.zero);

            var lookDirection = LookPosition - _rigidbody.position;
            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            _rigidbody.rotation = angle;
        }

        private void OnTriggerEnter2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                return;
            }
            _activeInteracts.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                return;
            }
            _activeInteracts.Remove(interactable);
        }

        private void HandleMove(Vector2 direction)
        {
            _movementDirection = direction;
        }

        private void HandleLookMouse(Vector2 mousePosition)
        {
            LookPosition = _camera.ScreenToWorldPoint(mousePosition);
        }

        private void HandleLookGamepad(Vector2 stickDirection)
        {
            var playerPosition = transform.position;
            LookPosition = playerPosition + new Vector3(stickDirection.x * 10f, stickDirection.y * 10f, playerPosition.z);
        }

        private void HandleInteract()
        {
            foreach (IInteractable interactable in _activeInteracts)
            {
                interactable.Interact(gameObject);
            }
        }

        private void HandleReload()
        {
            currentWeapon.Reload();

            _animator.Play("PlayerPlaceholder_HandGun_Reload");
        }

        private void OnReloadEnd()
        {
            currentWeapon.ReloadEnded();
        }

        private void HandleFire()
        {
            if (currentWeapon.CanShoot)
            {
                currentWeapon.Shoot();
            }
        }

    }
}