using System.Collections;
using Characters;
using Characters.Player;
using UnityEngine;
using UnityEngine.Pool;

namespace InteractableObjects.Weapons
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private LayerMask destroyLayers;
        [SerializeField] private float destroyTime = 5f;
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 20f;
        private Rigidbody2D _rigidBody;

        private ObjectPool<Bullet> _pool;
        private Coroutine _deactivateBulletCoroutine;
        private bool _isReleasedToPool; // Flag to check if bullet has already been released to the pool

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }
        
        private void OnEnable()
        {
            _isReleasedToPool = false;
            _deactivateBulletCoroutine = StartCoroutine(DeactivateBulletAfterTime(destroyTime));
        }
        
        private void OnDisable()
        {
            if (_deactivateBulletCoroutine != null)
            {
                StopCoroutine(_deactivateBulletCoroutine);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            if (_rigidBody != null) 
                _rigidBody.velocity = direction * speed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isReleasedToPool) return;

            if ((destroyLayers.value & (1 << collision.gameObject.layer)) > 0)
            {
                var healthComponent = collision.collider.GetComponent<HealthComponent>();

                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(damage);
                }

                ReleaseBullet();
            }
        }
        
        public void SetPool(ObjectPool<Bullet> pool)
        {
            _pool = pool;
        }
        
        private IEnumerator DeactivateBulletAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            ReleaseBullet();
        }

        private void ReleaseBullet()
        {
            if (_isReleasedToPool) return; // Prevent multiple releases
            _isReleasedToPool = true;
            _pool.Release(this);
        }
    }
}
