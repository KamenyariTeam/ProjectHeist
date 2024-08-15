using System.Collections.Generic;
using InteractableObjects;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerController : MonoBehaviour
    {
        private InputReader _input;
        
        private MovementComponent _movementComponent;
        private InteractionComponent _interactionComponent;
        private WeaponComponent _weaponComponent;
        private HealthComponent _healthComponent;

        private void Start()
        {
            InitializeComponents();
            SetupInputHandlers();
        }

        private void InitializeComponents()
        {
            _movementComponent = GetComponent<MovementComponent>();
            _interactionComponent = GetComponent<InteractionComponent>();
            _weaponComponent = GetComponent<WeaponComponent>();
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += OnDeathHandler;
        }

        private void SetupInputHandlers()
        {
            _input = ScriptableObject.CreateInstance<InputReader>();
            _input.MoveEvent += _movementComponent.HandleMove;
            _input.FireEvent += _weaponComponent.HandleFire;
            _input.ReloadEvent += _weaponComponent.HandleReload;
            _input.InteractEvent += _interactionComponent.HandleInteract;
            _input.UseEvent += _interactionComponent.HandleUse;
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