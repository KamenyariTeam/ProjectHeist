using System.Collections.Generic;
using InteractableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerController : MonoBehaviour, ICharacter
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        public float moveSpeed = 1f;

        // Pick up subsystem
        public BoxCollider2D interactBoxCollider;

        // Shooting
        public WeaponComponent currentWeapon;

        private Rigidbody2D _rigidbody;
        private PlayerInput _playerInput;
        private Vector2 _movementInput;
        private Vector2 _lookInput;
        private readonly List<IInteractable> _activeInteracts = new();

        // Animation
        private Animator _animator;

        public CharacterType GetCharacterType()
        {
            return CharacterType.Player;
        }

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

        private void OnMove(InputValue movementValue)
        {
            _movementInput = movementValue.Get<Vector2>();
        }

        private void OnInteract()
        {
            foreach (IInteractable interactable in _activeInteracts)
            {
                interactable.Interact(gameObject);
            }
        }

        private void OnReload()
        {
            currentWeapon.Reload();

            _animator.Play("PlayerPlaceholder_HandGun_Reload");
        }

        private void OnReloadEnd()
        {
            currentWeapon.ReloadEnded();
        }

        private void OnFire()
        {
            if (currentWeapon.CanShoot)
            {
                currentWeapon.Shoot();
            }
        }

    }
}