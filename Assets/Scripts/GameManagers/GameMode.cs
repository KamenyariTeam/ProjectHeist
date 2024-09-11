using Characters.Player;
using UnityEngine;

namespace GameManagers
{
    public class GameMode : MonoBehaviour
    {
        private PlayerController _playerController;
        public PlayerController PlayerController {
            get
            {
                if (_playerController == null)
                {
                    _playerController = FindObjectOfType<PlayerController>();
                }
                return _playerController;
            }
        }
        
    }
}