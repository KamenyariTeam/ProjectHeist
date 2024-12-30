using UnityEngine;

namespace Characters.AI
{

    public static class AIHelpers
    {
        public static float RotateToObject(Vector3 aiPosition, Vector3 objectPosition)
        {
            Vector3 direction = (objectPosition - aiPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle;
        }
    }
}