using UnityEngine;

namespace Map
{
    public class SurveillanceCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        public float rotationRange = 45f;
        public float rotationSpeed = 2f;
        public float viewDistance = 5f;
        public float viewAngle = 45f; // Angle of the view cone

        [Header("References")]
        public Transform playerTransform; // Reference to the player's transform

        private float _initialRotation;
        private bool _isRotatingRight;
        private SpriteRenderer _sprite;
        private bool _playerDetected;

        void Start()
        {
            _initialRotation = transform.eulerAngles.z;
            _isRotatingRight = true;
            _sprite = GetComponent<SpriteRenderer>();
            _playerDetected = false;
        }

        void Update()
        {
            if (!_playerDetected)
            {
                RotateCamera();
            }
            else
            {
                FollowPlayer();
            }
            DrawViewCone();
            DetectPlayer();
        }

        private void RotateCamera()
        {
            float rotationSpeedAdjusted = _isRotatingRight ? -rotationSpeed * Time.deltaTime : rotationSpeed * Time.deltaTime;
            float targetRotation = _isRotatingRight ? _initialRotation - rotationRange : _initialRotation + rotationRange;
            transform.Rotate(0, 0, rotationSpeedAdjusted);
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetRotation)) < Mathf.Abs(rotationSpeedAdjusted))
            {
                _isRotatingRight = !_isRotatingRight;
            }
        }

        private void FollowPlayer()
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            float step = rotationSpeed * Time.deltaTime;
            float newRotation = Mathf.MoveTowardsAngle(transform.eulerAngles.z, angleToPlayer, step);
            transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
        }

        private void DrawViewCone()
        {
            Debug.DrawLine(transform.position, transform.position + GetBoundary(viewAngle / 2), Color.red);
            Debug.DrawLine(transform.position, transform.position + GetBoundary(-viewAngle / 2), Color.red);
        }

        private Vector3 GetBoundary(float angle)
        {
            return Quaternion.Euler(0, 0, angle) * transform.up * viewDistance;
        }
        
        private void DetectPlayer()
        {
            Vector3 directionToPoint = playerTransform.position - transform.position;
            if (directionToPoint.magnitude > viewDistance)
            {
                _sprite.color = Color.green;
                _playerDetected = false;
                return;
            }

            float halfAngle = viewAngle / 2.0f;
            float angleToPoint = Vector3.Angle(transform.up, directionToPoint);

            if (Mathf.Abs(angleToPoint) <= halfAngle)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPoint, viewDistance);
                if (hit.collider)
                {
                    if (hit.collider.CompareTag(""))
                    {
                        
                    }
                    _sprite.color = Color.red;
                    Debug.Log("Player detected!");
                    _playerDetected = true;
                }
                else
                {
                    _sprite.color = Color.green;
                    _playerDetected = false;
                }
                _sprite.color = Color.red;
                Debug.Log("Player detected!");
                _playerDetected = true;
            }
            else
            {
                _sprite.color = Color.green;
                _playerDetected = false;
            }
        }
    }
}
