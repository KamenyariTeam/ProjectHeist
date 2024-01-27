using UnityEngine;

namespace Tools
{
    public class ScannerTool : MonoBehaviour, ITool
    {
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float maxUsageDistance;
        [SerializeField] private float originalRotation;
        [SerializeField] private float instantiatingDelta;
        [SerializeField] private GameObject scannerPrefab;

        public bool UseTool(GameObject player)
        {
            var playerController = player.GetComponent<Character.PlayerController>();
            if (playerController == null)
            {
                return false;
            }
            Vector2 position = new Vector2(player.transform.position.x, player.transform.position.y);
            Vector2 direction = (playerController.LookPosition - position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(position, direction, maxUsageDistance, wallMask);
            if (!hit)
            {
                return false;
            }

            Vector2 scannerPosition2D = hit.point - instantiatingDelta * direction;
            Vector3 scannerPosition = new Vector3(scannerPosition2D.x, scannerPosition2D.y, 0);

            float angle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
            Quaternion scannerRotation = Quaternion.Euler(0, 0, originalRotation + angle);

            Instantiate(scannerPrefab, scannerPosition, scannerRotation);

            return true;
        }

    }
}
