using UnityEngine;

namespace InteractableObjects.Door
{
    public class DoubleDoor : DoorBase
    {
        private Transform _doorLeftTransform, _doorRightTransform;
        private Vector3 _startPositionLeftSide, _endPositionLeftSide;
        private Vector3 _startPositionRightSide, _endPositionRightSide;

        protected override void Start()
        {
            base.Start();
            float doorWidth = GetComponentInChildren<Renderer>().localBounds.size.x;
            _doorLeftTransform = transform.Find("LeftDoor");
            _doorRightTransform = transform.Find("RightDoor");
            _startPositionLeftSide = _doorLeftTransform.position;
            _endPositionLeftSide = _startPositionLeftSide - _doorLeftTransform.right * doorWidth;
            _startPositionRightSide = _doorRightTransform.position;
            _endPositionRightSide = _startPositionRightSide + _doorRightTransform.right * doorWidth;
        }

        protected override void SlideDoors()
        {
            if (Timer < movementTime)
            {
                Timer += Time.deltaTime;
                _doorLeftTransform.position = Slide(IsOpened ? _startPositionLeftSide : _endPositionLeftSide,
                    IsOpened ? _endPositionLeftSide : _startPositionLeftSide, Timer / movementTime);
                _doorRightTransform.position = Slide(IsOpened ? _startPositionRightSide : _endPositionRightSide,
                    IsOpened ? _endPositionRightSide : _startPositionRightSide, Timer / movementTime);
            }
            else
            {
                ShouldSlide = false;
            }
        }
    }
}