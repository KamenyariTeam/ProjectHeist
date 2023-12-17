using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
