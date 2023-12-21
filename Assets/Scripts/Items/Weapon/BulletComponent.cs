using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{
    public float speed = 20f;
    public Vector2 direction;
    public Rigidbody2D rigidBody;

    void Start()
    {
        rigidBody.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null)
        {
            Destroy(gameObject);
        }
    }
}
