using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SuspicionMeter : MonoBehaviour
    {
        [SerializeField] private Image suspicionEye;
        [SerializeField] private Image suspicionBar;

        private bool _visible;
        private Vector3 _offset;
        private Transform _eyeTransform;
        private Transform _parentTransform;

        private void Start()
        {
            SetVisibility(false);

            _eyeTransform = transform;
            _parentTransform = _eyeTransform.root;
            _offset = _eyeTransform.localPosition;
        }

        private void LateUpdate()
        {
            if (_visible)
            {
                // Reset the GameObject's rotation to the initial rotation every frame.
                _eyeTransform.localRotation = Quaternion.Inverse(_parentTransform.rotation);

                // Calculate the correct position above the enemy
                var radius = _eyeTransform.localPosition.magnitude;
                var rotationAngle = Mathf.Deg2Rad * _eyeTransform.localRotation.eulerAngles.z + Mathf.PI / 2;
                _offset.x = radius * Mathf.Cos(rotationAngle);
                _offset.y = radius * Mathf.Sin(rotationAngle);
                _eyeTransform.localPosition = _offset;
            }
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
            suspicionEye.enabled = _visible;
            suspicionBar.enabled = _visible;
        }

        public void SetBarFillAmount(float percentage)
        {
            suspicionBar.fillAmount = percentage;
        }
    }
}