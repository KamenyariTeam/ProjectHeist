using Characters.Player;
using UnityEngine;

namespace Tools.Scanner
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
            var movementComponent = player.GetComponent<MovementComponent>();
            if (movementComponent == null)
            {
                return false;
            }

            var playerPosition3d = player.transform.position;
            Vector2 playerPosition2d = new Vector2(playerPosition3d.x, playerPosition3d.y);
            Vector2 direction = (movementComponent.LookPosition - playerPosition2d).normalized;
            RaycastHit2D hit = Physics2D.Raycast(playerPosition2d, direction, maxUsageDistance, wallMask);
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
