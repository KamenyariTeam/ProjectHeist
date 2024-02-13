using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuScript : BaseUIWindow
    {
        public void OnPressNewGame()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void OnPressSettings()
        {
            _uiManager.PushUI(EUIType.SettingsMenu);
        }

        public void OnPressQuit()
        {
            Application.Quit();
        }

    }
}