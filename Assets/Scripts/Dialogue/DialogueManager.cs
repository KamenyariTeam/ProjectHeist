using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using Characters.Player;
using GameManagers;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI dialogueText;

    public bool isPlayingDialogue { get; private set; }

    private Story _currentStory;

    private InputReader _input;

    private static DialogueManager _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("More than one instance of singleton DialogManager was created");
        }
        _instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return _instance;
    }

    private void Start()
    {
        isPlayingDialogue = false;
        dialoguePanel.SetActive(false);

        SetupInputHandlers();
    }

    //private void Update()
    //{
    //    if(!isPlayingDialogue)
    //    {
    //        return;
    //    }
    //}

    private void SetupInputHandlers()
    {
        var player = ManagersOwner.GetManager<GameMode>().PlayerController;
        _input = player.Input;
        _input.PressEvent += ContinueStory;
    }

    public void EnterDialogueMode(TextAsset inkJson)
    {
        _currentStory = new Story(inkJson.text);
        isPlayingDialogue = true;
        dialoguePanel.SetActive(true);

        ContinueStory();

        _input.SetUI();
    }

    private void ExitDialogueMode()
    {
        isPlayingDialogue = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        _input.SetGameplay();
    }

    private void ContinueStory()
    {
        if (_currentStory.canContinue)
        {
            dialogueText.text = _currentStory.Continue();
        }
        else
        {
            ExitDialogueMode();
        }
    }
}
