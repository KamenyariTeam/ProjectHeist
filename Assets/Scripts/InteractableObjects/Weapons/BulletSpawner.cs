using UnityEngine;
using UnityEngine.Pool;

namespace InteractableObjects.Weapons
{
    public class BulletSpawner : MonoBehaviour
    {
        public ObjectPool<Bullet> Pool;
        
        private BaseWeapon _weapon;
        
        private void Start()
        {
            _weapon = GetComponent<BaseWeapon>();
            Pool = new ObjectPool<Bullet>(CreateBullet, OnTakeBulletFromPool, OnReturnBulletToPool, OnDestroyBullet, true, 30, 100);
        }
        
        Bullet CreateBullet()
        {
            var bullet = Instantiate(_weapon.bullet, _weapon.BulletSpawnPoint.position, _weapon.BulletSpawnPoint.rotation);
            
            bullet.SetPool(Pool);
            
            return bullet;
        }
        
        // What do want to do when we take bullet from pool
        private void OnTakeBulletFromPool(Bullet bullet)
        {
            // Set bullet position and rotation
            bullet.transform.position = _weapon.BulletSpawnPoint.position;
            bullet.transform.rotation = _weapon.BulletSpawnPoint.rotation;
            
            bullet.gameObject.SetActive(true);
        }
        
        // What do we want to do when we return bullet to pool
        private void OnReturnBulletToPool(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }
        
        // What do we want to do when we destroy an object instead of returning it to the pool
        private void OnDestroyBullet(Bullet bullet)
        {
            Destroy(bullet.gameObject);
        }
    }
}