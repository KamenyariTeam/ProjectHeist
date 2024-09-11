using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public enum InputMode
    {
        GAMEPLAY,
        UI
    }

    public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IUIActions
    {
        private GameInput _gameInput;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();

                _gameInput.Gameplay.SetCallbacks(this);
                _gameInput.UI.SetCallbacks(this);

                SetGameplay();
            }
        }

        public void SetGameplay()
        {
            _gameInput.Gameplay.Enable();
            _gameInput.UI.Disable();
        }

        public void SetUI()
        {
            _gameInput.Gameplay.Disable();
            _gameInput.UI.Enable();
        }

        // Gameplay events
        public event Action<Vector2> MoveEvent;

        public event Action FireEvent;
        public event Action ReloadEvent;

        public event Action InteractEvent;
        public event Action UseEvent;
        
        public event Action PauseEvent;

        public event Action SaveGameEvent;
        public event Action LoadGameEvent;

        // UI events
        public event Action ResumeEvent;
        public event Action AcceptEvent;

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
                FireEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                InteractEvent?.Invoke();
        }

        public void OnUse(InputAction.CallbackContext context)
        {
            if (context.performed)
                UseEvent?.Invoke();
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
                ReloadEvent?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PauseEvent?.Invoke();
                SetUI();
            }
        }

        public void OnSaveGame(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SaveGameEvent?.Invoke();
            }
        }

        public void OnLoadGame(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                LoadGameEvent?.Invoke();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ResumeEvent?.Invoke();
                SetGameplay();
            }
        }

        public void OnAccept(InputAction.CallbackContext context)
        {
            if (context.performed)
                AcceptEvent?.Invoke();
        }
    }
}