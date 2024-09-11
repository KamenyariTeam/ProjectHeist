using InteractableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Visual Cue")]
    [SerializeField]
    private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField]
    private TextAsset inkJson;

    private bool _isSelected;

    public bool IsSelected { get => _isSelected; set => _isSelected = value; }

    private void Awake()
    {
        visualCue.SetActive(false);
    }

    public void Interact(GameObject character)
    {
        if (!DialogueManager.GetInstance().isPlayingDialogue)
        {
            DialogueManager.GetInstance().EnterDialogueMode(inkJson);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !DialogueManager.GetInstance().isPlayingDialogue)
        {
            visualCue.SetActive(true);
        }    
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            visualCue.SetActive(false);
        }
    }
}
