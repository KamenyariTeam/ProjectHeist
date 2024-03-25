using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MapIcon : MonoBehaviour
    {
        private MissionMap _missionMap;

        public string Title;
        public Image Image;
        public string LevelName;

        private void Start()
        {
            _missionMap = GetComponentInParent<MissionMap>();
        }

        public void OnSelected()
        {
            _missionMap?.SetSelectedIcon(this);
        }
    }
}
