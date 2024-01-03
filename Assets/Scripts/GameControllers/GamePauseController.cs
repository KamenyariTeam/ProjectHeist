using UnityEngine;
using UnityEngine.Serialization;

public class GamePauseController : MonoBehaviour
{
    [SerializeField]
    private Texture2D crosshairCursor;
    [FormerlySerializedAs("standardCursor")] [SerializeField]
    private Texture2D menuCursor;

    private bool _isPaused;
    
    void Start()
    {
        OnApplicationFocus(true);
    }
    
    void OnApplicationPause(bool pauseStatus)
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

    void OnApplicationFocus(bool hasFocus)
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
