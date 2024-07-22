using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class SurveillanceCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float rotationRange = 45f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float viewDistance = 2f;
        [SerializeField] private float deadZoneDistance = 0.5f; // Inner radius of the truncated cone
        [SerializeField] private float viewAngle = 45f; // Angle of the view cone
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private Color searchStateColor = Color.green;
        [SerializeField] private Color detectedStateColor = Color.red;

        [Header("References")]
        [SerializeField] private Transform playerTransform; // Reference to the player's transform

        private float _initialRotation;
        private bool _isRotatingRight;
        private bool _playerDetected;
        private Mesh _viewConeMesh;
        private MeshRenderer _meshRenderer;

        private void OnValidate()
        {
            InitializeMesh(); // Regenerate the view cone mesh when values change in the Inspector
        }

        private void Start()
        {
            _initialRotation = transform.eulerAngles.z;
            _isRotatingRight = true;
            _playerDetected = false;

            InitializeMesh();
        }

        void InitializeMesh()
        {
            MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshFilter != null && _meshRenderer != null)
            {
                meshFilter.mesh = _viewConeMesh = new Mesh();
                _meshRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = searchStateColor };
                
                CreateViewConeMesh();
            }
            else
            {
                Debug.LogError("MeshFilter or MeshRenderer not found in the children of the SurveillanceCamera object.");
            }
        }

        private void Update()
        {
            if (!_playerDetected)
            {
                RotateCamera();
            }
            else
            {
                FollowPlayer();
            }
            DetectPlayer();
        }

        // Rotate the camera back and forth within the specified range
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

        // Follow the player by smoothly rotating the camera towards them
        private void FollowPlayer()
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            float step = rotationSpeed * Time.deltaTime;
            float newRotation = Mathf.MoveTowardsAngle(transform.eulerAngles.z, angleToPlayer, step);

            if (Mathf.Abs(Mathf.DeltaAngle(newRotation, _initialRotation)) < rotationRange)
                transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
        }

        // Create the view cone mesh for visualization
        private void CreateViewConeMesh()
        {
            int segments = 20;
            float angleStep = viewAngle / segments;
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            int[] triangles = new int[segments * 6];

            // Generate outer and inner vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = -viewAngle / 2 + angleStep * i;
                vertices[i] = Quaternion.Euler(0, 0, angle) * Vector3.up * viewDistance;
                vertices[i + segments + 1] = Quaternion.Euler(0, 0, angle) * Vector3.up * deadZoneDistance;
            }

            // Generate triangles
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 6;
                
                // Define the first triangle (outer vertex, inner vertex, next outer vertex)
                triangles[baseIndex] = i;
                triangles[baseIndex + 1] = i + segments + 1;
                triangles[baseIndex + 2] = i + 1;
                
                // Define the second triangle (next outer vertex, inner vertex, next inner vertex)
                triangles[baseIndex + 3] = i + 1;
                triangles[baseIndex + 4] = i + segments + 1;
                triangles[baseIndex + 5] = i + segments + 2;
            }

            _viewConeMesh.Clear();
            _viewConeMesh.vertices = vertices;
            _viewConeMesh.triangles = triangles;
            _viewConeMesh.RecalculateNormals();
        }

        // Detect the player within the camera's view cone
        private void DetectPlayer()
        {
            Vector3 directionToPoint = playerTransform.position - transform.position;
            float distanceToPoint = directionToPoint.magnitude;

            if (distanceToPoint > viewDistance || distanceToPoint < deadZoneDistance)
            {
                SetSearchState();
                return;
            }

            float halfAngle = viewAngle / 2.0f;
            float angleToPoint = Vector3.Angle(transform.up, directionToPoint);

            if (Mathf.Abs(angleToPoint) <= halfAngle)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPoint, distanceToPoint, obstacleMask);
                if (!hit.collider)
                {
                    SetDetectedState();
                    Debug.Log("Player detected!");
                }
                else
                {
                    SetSearchState();
                }
            }
            else
            {
                SetSearchState();
            }
        }

        private void SetSearchState()
        {
            _meshRenderer.material.color = searchStateColor;
            _playerDetected = false;
        }

        private void SetDetectedState()
        {
            _meshRenderer.material.color = detectedStateColor;
            _playerDetected = true;
        }
    }
}
