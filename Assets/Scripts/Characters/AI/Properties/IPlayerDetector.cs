using Characters.Player;
using UnityEngine;

namespace Characters.AI
{
    public interface IPlayerDetector
    {
        public bool IsPlayerInSight { get; }
        public Transform PlayerTransform { get; }
        public StealthComponent PlayerStealthComponent { get; }

        public float SuspicionLevel { get; set; }
        public float SuspicionIncreaseRate { get; }

        public bool IsOnAlert { get; set; }

    }
}