using System.Collections.Generic;
using InteractableObjects;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerController : MonoBehaviour, ISavableComponent
    {
        // Animation
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly string ReloadAnimationName = "PlayerPlaceholder_HandGun_Reload";
        private Animator _animator;
        private HealthComponent _healthComponent;

        // Movement
        public float moveSpeed = 1f;
        private Rigidbody2D _rigidbody;
        private Vector2 _movementDirection;

        // Look and Aim
        private UnityEngine.Camera _camera;
        private Vector2 _lookPosition;
        public Vector2 LookPosition => _lookPosition;

        // Interactable Handling
        [SerializeField] private LayerMask wallLayer;
        private readonly List<IInteractable> _activeInteracts = new();

        // Weapon and Tool Handling
        public WeaponComponent currentWeapon;
        public GameObject currentTool;

        // Input Handling
        private InputReader _input;

        private void Start()
        {
            InitializeComponents();
            SetupInputHandlers();
        }

        private void InitializeComponents()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _camera = UnityEngine.Camera.main;
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHendler;
        }

        private void SetupInputHandlers()
        {
            _input = ScriptableObject.CreateInstance<InputReader>();
            _input.MoveEvent += HandleMove;
            _input.FireEvent += HandleFire;
            _input.ReloadEvent += HandleReload;
            _input.InteractEvent += HandleInteract;
            _input.UseEvent += HandleUse;
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
            UpdateSelectedInteractable();
        }

        private void UpdateLookPosition()
        {
            _lookPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void UpdateMovement()
        {
            _rigidbody.velocity = _movementDirection * moveSpeed;
            _animator.SetBool(IsMoving, _movementDirection != Vector2.zero);
        }

        private void UpdateRotation()
        {
            var lookDirection = _lookPosition - _rigidbody.position;
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
            interactable.IsSelected = false;
            UpdateSelectedInteractable();
        }

        private void HandleMove(Vector2 direction)
        {
            _movementDirection = direction;
        }

        private void HandleInteract()
        {
            IInteractable selected = _activeInteracts.Find(interactable => interactable.IsSelected);
            selected?.Interact(gameObject);
        }

        private void HandleUse()
        {
            if (currentTool)
            {
                var tool = currentTool.GetComponent<Tools.ITool>();
                tool?.UseTool(gameObject);
            }
        }

        private void HandleReload()
        {
            currentWeapon.Reload();

            _animator.Play(ReloadAnimationName);
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

        private void UpdateSelectedInteractable()
        {
            foreach (IInteractable interactable in _activeInteracts)
            {
                interactable.IsSelected = false;
            }

            var selectedInteractable = GetCurrentInteractable();
            if (selectedInteractable != null)
            {
                selectedInteractable.IsSelected = true;
            }
        }

        private IInteractable GetCurrentInteractable()
        {
            IInteractable closestInteractable = null;
            float closestDist = float.MaxValue;

            foreach (IInteractable interactable in _activeInteracts)
            {
                if (interactable is MonoBehaviour component)
                {
                    if (IsComponentVisible(component))
                    {
                        float dist = Vector2.Distance(_lookPosition, component.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestInteractable = interactable;
                        }
                    }
                }
            }

            return closestInteractable;
        }

        private bool IsComponentVisible(MonoBehaviour component)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, component.transform.position, wallLayer);
            if (!hit)
            {
                return true;
            }

            GameObject hitObject = hit.collider.gameObject;
            GameObject interactableObject = component.gameObject;

            // we hit the same object or a child of the interactable
            return hitObject.GetInstanceID() == interactableObject.GetInstanceID()
                   || hitObject.transform.IsChildOf(interactableObject.transform);
        }

        private void OnDeathHendler()
        {
            // TODO Idea move this logic to some kind of game mode where we could handle all necessary logic, like cursor switch etc.
            SceneManager.LoadScene("MainMenuScene");
        }

        public ComponentData Serialize()
        {
            var data = new ExtendedComponentData();
            data.SetTransform("transform", transform);
            return data;
        }

        public void Deserialize(ComponentData data)
        {
            if (data is ExtendedComponentData unpacked)
            {
                unpacked.GetTransform("transform", transform);
            }
        }
    }
}