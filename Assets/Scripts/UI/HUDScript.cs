using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    public GameObject player;
    public TMP_Text text;

    private WeaponComponent _weaponComponent;

    void Start()
    {
        _weaponComponent = player.GetComponent<WeaponComponent>();
    }

    void Update()
    {
        text.SetText(_weaponComponent.currentAmmo + " / " + _weaponComponent.maxAmmo);
    }
}
