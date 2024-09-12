using UnityEngine;

namespace Characters
{
    public class AnimationComponent : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void PlayAnimation(AnimationClip animationClip)
        {
            if (animationClip)
            {
                _animator.Play(animationClip.name);
            }
        }

        public void PlayAnimation(string animationName)
        {
            _animator.Play(animationName);
        }

        public void UpdateMovementAnimation(bool isMoving)
        {
            _animator.SetBool(IsMoving, isMoving);
        }
    }
}
