using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;

namespace Characters.AI
{
    public interface IAIPlayerDetectable
    {
        public bool IsPlayerInSight { get; }
        public float SuspicionLevel { get; set; }
        public StealthComponent PlayerStealthComponent { get; }
        public bool IsOnAlert { get; set; }
        public float SuspicionIncreaseRate { get; }
        public Transform PlayerTransform { get; }
    }
}