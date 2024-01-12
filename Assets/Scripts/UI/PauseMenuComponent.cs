using GameControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuComponent : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject gameHudUI;
    public GamePauseController gamePauseController;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        gameHudUI.SetActive(false);
    }

    void Update()
    {
        
    }

    public void OnPressResume()
    {
        //SceneManager.LoadScene("SampleScene");
    }

    public void OnPressSave()
    {
        // SceneManager.LoadScene("SettingsScene");
    }

    public void OnPressLoad()
    {
        Application.Quit();
    }

    public void OnPressMainMenu()
    {
        Application.Quit();
    }
}
