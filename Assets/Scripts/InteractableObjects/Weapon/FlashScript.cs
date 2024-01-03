using UnityEngine;

namespace InteractableObjects.Weapon
{
    public class FlashScript : MonoBehaviour
    {
        public void OnAnimationEndPlay()
        {
            Destroy(gameObject);
        }
    }
}