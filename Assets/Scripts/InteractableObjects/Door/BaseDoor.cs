using Characters.AI;
using UnityEngine;

namespace InteractableObjects.Door
{
    public abstract class DoorBase : OutlinedInteractable
    {
        [SerializeField] protected float maxTimeOpened = 3.0f; // Time before the door closes
        [SerializeField] protected float movementTime = 1.0f; // Duration of the slide
        protected float Timer;
        protected bool ShouldSlide;
        protected bool IsOpened;
        
        private float _timeOpened;

        public override void Interact(GameObject interacter)
        {
            if (!ShouldSlide)
            {
                var isAI = interacter.GetComponent<IAILogic>() != null;
                if (!isAI || !IsOpened)
                {
                    ChangeState();
                }
            }
        }

        protected void Update()
        {
            if (IsOpened)
            {
                _timeOpened += Time.deltaTime;
                if (_timeOpened > maxTimeOpened)
                {
                    ChangeState();
                }
            }

            if (ShouldSlide)
            {
                SlideDoors();
            }
        }

        protected abstract void SlideDoors();

        protected Vector3 Slide(Vector3 start, Vector3 end, float fraction)
        {
            return Vector3.Lerp(start, end, fraction);
        }

        private void ChangeState()
        {
            IsOpened = !IsOpened;
            Timer = 0;
            ShouldSlide = true;
            _timeOpened = 0.0f;
        }
    }
}