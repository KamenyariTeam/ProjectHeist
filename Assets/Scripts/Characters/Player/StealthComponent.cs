using Map;
using UnityEngine;

namespace Characters.Player
{
    public class StealthComponent : MonoBehaviour
    {
        [SerializeField] private int acceptableNoticeability = 20;
        public int noticeability;

        private MapArea _currentMapArea;
        private MapArea _nextMapArea;

        public bool IsNoticeable => IsInRestrictedArea() || noticeability > acceptableNoticeability;

        private bool IsInRestrictedArea()
        {
           return _currentMapArea != null && _currentMapArea.IsRestricted;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("MapArea"))
            {
                MapArea newMapArea = other.GetComponent<MapArea>();

                if(_currentMapArea == null)
                {
                    EnterArea(newMapArea);
                }
                else
                {
                    _nextMapArea = newMapArea;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("MapArea"))
            {
                MapArea exitingMapArea = other.GetComponent<MapArea>();

                if(exitingMapArea == _currentMapArea)
                {
                    HandleCurrentAreaExit();
                }
                else if (exitingMapArea == _nextMapArea)
                {
                    HandleNextAreaExit();
                }
            }
        }

        private void EnterArea(MapArea mapArea)
        {
            Debug.Log($"Enter {mapArea.AreaName}. Area restricted: {mapArea.IsRestricted}");
            _currentMapArea = mapArea;
            _nextMapArea = null;
        }

        private void HandleCurrentAreaExit()
        {
            if (_nextMapArea != null)
            {
                EnterArea(_nextMapArea);
            }
            else
            {
                Debug.Log($"You left {_currentMapArea.AreaName}");
                _currentMapArea = null;
            }
        }
        
        private void HandleNextAreaExit()
        {
            Debug.Log($"You didn't entered {_nextMapArea.AreaName}");
            _nextMapArea = null;
        }
    }
}
