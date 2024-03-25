using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class MissionMap : MonoBehaviour
    {
        private MapIcon[] _mapIcons;
        private MapIcon _selectedMapIcon;
        private MapPanel _missionPanel;

        // Start is called before the first frame update
        void Start()
        {
            _mapIcons = GetComponentsInChildren<MapIcon>();
            _missionPanel = GetComponentInChildren<MapPanel>(true);
            SetVisible(false);
        }

        public void SetSelectedIcon(MapIcon icon)
        {
            _selectedMapIcon = icon;
            if (icon)
            {
                _missionPanel.SetVisible(true);
            }
        }

        public MapIcon GetSelectedIcon()
        {
            return _selectedMapIcon;
        }

        public void SetVisible(bool isVisible)
        {
            if (isVisible)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
