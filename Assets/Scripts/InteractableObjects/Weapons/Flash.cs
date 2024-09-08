using UnityEngine;
using UnityEngine.Pool;

namespace InteractableObjects.Weapons
{
    public class Flash : MonoBehaviour
    {
        private ObjectPool<Flash> _pool;
        
        public void OnAnimationEndPlay()
        {
            _pool.Release(this);
        }
        
        public void SetPool(ObjectPool<Flash> pool)
        {
            _pool = pool;
        }
    }
}