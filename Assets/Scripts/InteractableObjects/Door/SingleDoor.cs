using UnityEngine;

namespace InteractableObjects.Door
{
    public class SingleDoor : DoorBase
    {
        private Transform _doorTransform;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        protected override void Start()
        {
            base.Start();
            float doorWidth = GetComponentInChildren<Renderer>().localBounds.size.x;
            _doorTransform = transform.Find("Door");
            _startPosition = _doorTransform.position;
            _endPosition = _startPosition - _doorTransform.right * doorWidth;
        }

        protected override void SlideDoors()
        {
            if (Timer < movementTime)
            {
                Timer += Time.deltaTime;
                _doorTransform.position = Slide(IsOpened ? _startPosition : _endPosition,
                    IsOpened ? _endPosition : _startPosition, Timer / movementTime);
            }
            else
            {
                ShouldSlide = false;
            }
        }
    }
}