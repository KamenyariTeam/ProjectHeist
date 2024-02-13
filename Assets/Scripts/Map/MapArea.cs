using UnityEngine;

namespace Map
{
    public class MapArea : MonoBehaviour
    {
        [SerializeField] private bool isRestricted;
        [SerializeField] private string areaName;

        public bool IsRestricted => isRestricted;
        public string AreaName => areaName;
    }
}
