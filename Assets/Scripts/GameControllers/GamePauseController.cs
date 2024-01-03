using UnityEngine;

namespace GameControllers
{
    public class GamePauseController : MonoBehaviour
    {
        [SerializeField] private Texture2D crosshairCursor;
        [SerializeField] private Texture2D menuCursor;

        private bool _isPaused;

        private void Start()
        {
            OnApplicationFocus(true);
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
            _isPaused = !hasFocus;
            OnApplicationPause(_isPaused);
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