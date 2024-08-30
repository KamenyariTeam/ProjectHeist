using UnityEngine;

namespace Characters
{
    public class AnimationComponent : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly string ReloadAnimationName = "PlayerPlaceholder_HandGun_Reload";
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdateMovementAnimation(bool isMoving)
        {
            _animator.SetBool(IsMoving, isMoving);
        }

        public void PlayReloadAnimation()
        {
            _animator.Play(ReloadAnimationName);
        }
    }
}
