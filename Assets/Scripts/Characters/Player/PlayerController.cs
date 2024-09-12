using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerController : MonoBehaviour
    {
        public InputReader Input { get; private set; }
        
        private MovementComponent _movementComponent;
        private InteractionComponent _interactionComponent;
        private PlayerWeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        private void Awake()
        {
            InitializeComponents();
            SetupInputHandlers();
        }

        private void InitializeComponents()
        {
            _movementComponent = GetComponent<MovementComponent>();
            _interactionComponent = GetComponent<InteractionComponent>();
            _weaponComponent = GetComponent<PlayerWeaponComponent>();
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHandler;
        }

        private void SetupInputHandlers()
        {
            Input = new InputReader();
            Input.MoveEvent += _movementComponent.HandleMove;
            Input.SneakEvent += _movementComponent.HandleSneak;
            Input.FireEvent += _weaponComponent.HandleFire;
            Input.ReloadEvent += _weaponComponent.HandleReload;
            Input.ThrowWeaponEvent += _weaponComponent.HandleThrowWeapon;
            Input.InteractEvent += _interactionComponent.HandleInteract;
            Input.UseEvent += _interactionComponent.HandleUse;
        }

        private void OnDeathHandler()
        {
            // TODO: Move this logic to some kind of game mode where we could handle all necessary logic, like cursor switch etc.
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