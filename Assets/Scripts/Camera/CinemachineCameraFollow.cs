using System;
using Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CinemachineCameraFollow : MonoBehaviour
    {
        [SerializeField]
        private float boundaryRadius = 3f; // Radius of the circle boundary

        private UnityEngine.Camera _camera;
        private GameObject _player;
        private GameObject _pointToFollow;
    
        // Start is called before the first frame update
        void Start()
        {
            _camera = UnityEngine.Camera.current;
            _player = GameObject.FindWithTag("Player");

            _pointToFollow = new GameObject("PointToFollow");
            
            var virtualCamera = GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = _pointToFollow.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (_camera && _player)
            {
                var playerPosition = _player.transform.position;
                Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z - playerPosition.z));
                Vector3 midpoint = (playerPosition + mouseWorldPosition) / 2;

                // Check if midpoint is outside the boundary circle
                var distanceFromPlayer = Math.Sqrt(Math.Pow(midpoint.x - playerPosition.x, 2) + Math.Pow(midpoint.y - playerPosition.y, 2));
                if (distanceFromPlayer >= boundaryRadius)
                {
                    // Clamp to the edge of the circle
                    Vector3 fromPlayerToMidpoint = midpoint - playerPosition;
                    fromPlayerToMidpoint = fromPlayerToMidpoint.normalized * boundaryRadius;
                    _pointToFollow.transform.position = playerPosition + fromPlayerToMidpoint;
                }
                else
                {
                    _pointToFollow.transform.position = midpoint;
                }
            }
            else
            {
                _camera = UnityEngine.Camera.current;
                _player = GameObject.FindWithTag("Player");
            }
        }

        private bool IsCursorInGameWindow()
        {
            return Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
                   Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height;
        }
    }
}
