using UnityEngine;
using UnityEngine.Pool;

namespace InteractableObjects.Weapons
{
    public abstract class WeaponObjectSpawner<T> : MonoBehaviour where T : MonoBehaviour
    {
        public ObjectPool<T> Pool;
        protected BaseWeapon weapon; // Use 'protected' to allow derived classes to access it

        protected virtual void Start()
        {
            weapon = GetComponent<BaseWeapon>();
            Pool = new ObjectPool<T>(CreateObject, OnTakeObjectFromPool, OnReturnObjectToPool, OnDestroyObject, true, 30, 100);
        }
        
        // Method to provide the specific prefab for the object
        protected abstract T GetPrefab();

        private T CreateObject()
        {
            T obj = Instantiate(GetPrefab(), weapon.BulletSpawnPoint.position, weapon.BulletSpawnPoint.rotation);
            
            
            return obj;
        }

        private void OnTakeObjectFromPool(T obj)
        {
            obj.transform.position = weapon.BulletSpawnPoint.position;
            obj.transform.rotation = weapon.BulletSpawnPoint.rotation;
            obj.gameObject.SetActive(true);
        }

        private void OnReturnObjectToPool(T obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnDestroyObject(T obj)
        {
            Destroy(obj.gameObject);
        }
    }
}