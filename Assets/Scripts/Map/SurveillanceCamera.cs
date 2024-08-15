using System.Collections.Generic;
using Characters.Player;
using GameManagers;
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
        private Transform _playerTransform;
        private StealthComponent _playerStealthComponent;

        private float _initialRotation;
        private bool _isRotatingRight;
        private bool _playerDetected;
        private Mesh _viewConeMesh;
        private MeshRenderer _meshRenderer;

        private void OnValidate()
        {
            if (!Application.isPlaying && Application.isEditor)
                InitializeMesh(); // Regenerate the view cone mesh when values change in the Inspector
        }

        private void Start()
        {
            InitializeReferences();
            InitializeMesh();
        }

        private void InitializeReferences()
        {
            var player = ManagersOwner.GetManager<GameMode>().PlayerController;
            _playerTransform = player.transform;
            _playerStealthComponent = player.GetComponent<StealthComponent>();

            _initialRotation = transform.eulerAngles.z;
            _isRotatingRight = true;
            _playerDetected = false;
        }

        private void InitializeMesh()
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
            Vector3 directionToPlayer = _playerTransform.position - transform.position;
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
            if (!_playerStealthComponent.IsNoticeable)
            {
                SetSearchState(); // Player is not noticeable, continue searching
                return;
            }

            Vector3 directionToPlayer = _playerTransform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (IsPlayerOutOfView(directionToPlayer, distanceToPlayer) || 
                IsObstacleBlockingView(directionToPlayer, distanceToPlayer))
            {
                SetSearchState(); // Player is out of view or an obstacle is blocking, continue searching
            }
            else
            {
                SetDetectedState(); // Player detected
                Debug.Log("Player detected!");
            }
        }

        // Check if the player is out of the camera's view range or view cone
        private bool IsPlayerOutOfView(Vector3 directionToPlayer, float distanceToPlayer)
        {
            if (distanceToPlayer > viewDistance || distanceToPlayer < deadZoneDistance)
            {
                return true; // Player is out of the view distance range
            }

            float halfViewAngle = viewAngle / 2.0f;
            float angleToPlayer = Vector3.Angle(transform.up, directionToPlayer);
            return Mathf.Abs(angleToPlayer) > halfViewAngle; // Player is out of the view cone angle
        }

        // Check if an obstacle is blocking the view to the player
        private bool IsObstacleBlockingView(Vector3 directionToPlayer, float distanceToPlayer)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);
            return hit.collider != null; // There is an obstacle blocking the view
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
