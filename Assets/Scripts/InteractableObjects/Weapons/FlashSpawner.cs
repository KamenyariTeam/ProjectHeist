using UnityEngine;
using UnityEngine.Pool;

namespace InteractableObjects.Weapons
{
    public class FlashSpawner : MonoBehaviour
    {
        public ObjectPool<Flash> Pool;
        
        private BaseWeapon _weapon;
        
        private void Start()
        {
            _weapon = GetComponent<BaseWeapon>();
            Pool = new ObjectPool<Flash>(CreateFlash, OnTakeFlashFromPool, OnReturnFlashToPool, OnDestroyFlash, true, 30, 100);
        }
        
        Flash CreateFlash()
        {
            var flash = Instantiate(_weapon.flash, _weapon.BulletSpawnPoint.position, _weapon.BulletSpawnPoint.rotation);
            
            flash.SetPool(Pool);
            
            return flash;
        }
        
        // What do want to do when we take flash from pool
        private void OnTakeFlashFromPool(Flash flash)
        {
            // Set flash position and rotation
            flash.transform.position = _weapon.BulletSpawnPoint.position;
            flash.transform.rotation = _weapon.BulletSpawnPoint.rotation;
            
            flash.gameObject.SetActive(true);
        }
        
        // What do we want to do when we return flash to pool
        private void OnReturnFlashToPool(Flash flash)
        {
            flash.gameObject.SetActive(false);
        }
        
        // What do we want to do when we destroy an object instead of returning it to the pool
        private void OnDestroyFlash(Flash flash)
        {
            Destroy(flash.gameObject);
        }
    }
}