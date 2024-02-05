using Characters.Player;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 20f;
    public Vector2 direction;
    public Rigidbody2D rigidBody;

    private void Start()
    {
        rigidBody.velocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var healthComponent = collision.collider.GetComponent<HealthComponent>();

        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage);
        }

        if (collision != null)
        {
            Destroy(gameObject);
        }
    }
}