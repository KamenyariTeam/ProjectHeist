using Character;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HUDScript : MonoBehaviour
    {
        public GameObject player;
        public TMP_Text text;

        private WeaponComponent _weaponComponent;

        private void Start()
        {
            _weaponComponent = player.GetComponent<WeaponComponent>();
        }

        private void Update()
        {
            text.SetText(_weaponComponent.CurrentAmmo + " / " + _weaponComponent.maxAmmo);
        }
    }
}