using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MapPanel : MonoBehaviour
    {
        public TMP_Text LevelTitleText;
        private bool _isVisible;
        private MissionMap _missionMap;

        // Start is called before the first frame update
        void Start()
        {
            _missionMap = GetComponentInParent<MissionMap>();
            SetVisible(false);
        }

        public void Close()
        {
            _missionMap.SetSelectedIcon(null);
            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            if (isVisible)
            {
                SetProperties(_missionMap.GetSelectedIcon());

                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPressStartLevel()
        {
            MapIcon icon = _missionMap.GetSelectedIcon();
            SceneManager.LoadScene(icon.LevelName);
        }

        private void SetProperties(MapIcon icon)
        {
            LevelTitleText.SetText(icon.Title);
        }
    }
}
