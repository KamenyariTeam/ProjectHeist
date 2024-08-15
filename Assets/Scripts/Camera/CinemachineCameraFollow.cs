using Characters.Player;
using Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CinemachineCameraFollow : MonoBehaviour
    {
        private GameObject _player;
        private GameObject _pointToFollow;
        private MovementComponent _movementComponent;

        // Start is called before the first frame update
        void Start()
        {
            _player = GameObject.FindWithTag("Player");
            _movementComponent = _player.GetComponent<MovementComponent>();

            _pointToFollow = new GameObject("PointToFollow");
            
            var virtualCamera = GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = _pointToFollow.transform;
        }

        // Update is called once per frame
        void Update()
        {
            var playerPosition = _player.transform.position;
            var lookPosition = _movementComponent.LookPosition;
            var targetPosition = Vector3.Lerp(playerPosition, lookPosition, 1/3f);

            _pointToFollow.transform.position = targetPosition;
        }
    }
}
