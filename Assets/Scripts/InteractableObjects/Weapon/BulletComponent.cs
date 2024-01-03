using UnityEngine;

namespace InteractableObjects.Weapon
{
    public class BulletComponent : MonoBehaviour
    {
        public float speed = 20f;
        public Vector2 direction;
        public Rigidbody2D rigidBody;

        private void Start()
        {
            rigidBody.velocity = direction * speed;
        }

        private void OnTriggerEnter2D(Collider2D triggeredCollider)
        {
            if (triggeredCollider != null) Destroy(gameObject);
        }
    }
}