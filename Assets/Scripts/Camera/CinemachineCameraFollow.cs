using System;
using Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CinemachineCameraFollow : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private GameObject _player;
        private GameObject _pointToFollow;
    
        // Start is called before the first frame update
        void Start()
        {
            _camera = UnityEngine.Camera.main;
            _player = GameObject.FindWithTag("Player");

            _pointToFollow = new GameObject("PointToFollow");
            
            var virtualCamera = GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = _pointToFollow.transform;
            
        }

        // Update is called once per frame
        void Update()
        {
            var playerPosition = _player.transform.position;
            Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerPosition.z));
            Vector3 targetPosition = Vector3.Lerp(playerPosition, mouseWorldPosition, 1/3f);

            _pointToFollow.transform.position = targetPosition;
        }
    }
}
