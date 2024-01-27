using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MenuGameMode : MonoBehaviour
    {
        public void OnPressNewGame()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void OnPressSettings()
        {
            // SceneManager.LoadScene("SettingsScene");
        }

        public void OnPressQuit()
        {
            Application.Quit();
        }
    }
}