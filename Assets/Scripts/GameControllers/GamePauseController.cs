using Characters.Player;
using UnityEngine;

namespace GameControllers
{
    public class GamePauseController : MonoBehaviour
    {
        [SerializeField] private Texture2D crosshairCursor;
        [SerializeField] private Texture2D menuCursor;

        private InputReader _input;
        private bool _isPaused;

        private void Start()
        {
            OnApplicationFocus(true);
            
            _input = ScriptableObject.CreateInstance<InputReader>();
            
            // Setup inputs
            _input.PauseEvent += HandlePause;
            _input.ResumeEvent += HandlePause;
        }

        private void HandlePause()
        {
            OnApplicationPause(!_isPaused);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _isPaused = pauseStatus;
            if (pauseStatus)
            {
                Time.timeScale = 0f;
                SetMenuCursor();
            }
            else
            {
                Time.timeScale = 1f;
                SetCrosshairCursor();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationPause(!hasFocus);
        }

        private void SetCrosshairCursor()
        {
            Cursor.SetCursor(crosshairCursor, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void SetMenuCursor()
        {
            Cursor.SetCursor(menuCursor, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}