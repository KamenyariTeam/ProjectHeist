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
            InitializeInput();
            UpdatePauseState(false);
        }

        private void InitializeInput()
        {
            _input = ScriptableObject.CreateInstance<InputReader>();
            _input.PauseEvent += HandlePause;
            _input.ResumeEvent += HandlePause;
        }

        private void HandlePause()
        {
            UpdatePauseState(!_isPaused);
        }

        private void UpdatePauseState(bool pauseStatus)
        {
            _isPaused = pauseStatus;
            Time.timeScale = pauseStatus ? 0f : 1f;
            UpdateCursor(pauseStatus);
        }

        private void UpdateCursor(bool isPaused)
        {
            if (isPaused)
            {
                SetCursor(menuCursor, CursorLockMode.None);
            }
            else
            {
                SetCursor(crosshairCursor, CursorLockMode.Confined);
            }
        }

        private void SetCursor(Texture2D cursorTexture, CursorLockMode lockMode)
        {
            Vector2 hotSpot = Vector2.zero;
            if (cursorTexture)
            {
                hotSpot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            }

            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
            Cursor.lockState = lockMode;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            UpdatePauseState(!hasFocus);
        }
    }
}