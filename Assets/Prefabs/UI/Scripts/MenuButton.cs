using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;
    public Button button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = button.colors.highlightedColor; //Or however you do your color
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = button.colors.normalColor; //Or however you do your color
    }
}
