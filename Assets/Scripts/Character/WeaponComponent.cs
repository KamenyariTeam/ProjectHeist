using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponComponent : MonoBehaviour
{
    public float damage = 1f;
    public float maxAmmo = 7f;
    public float angleUntolerance = 1.5f;

    public float oneShotTime = 1f;

    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject flashFrefab;

    private float _currentAmmo = 7f;
    private float _timeSinceLastShoot;
    private bool _canShoot;

    void Start()
    {
        _currentAmmo = maxAmmo;
        _canShoot = true;
    }

    void Update()
    {
        if (!_canShoot)
        {
            if (_timeSinceLastShoot < oneShotTime)
            {
                _timeSinceLastShoot += Time.deltaTime;
            }
            else
            {
                _timeSinceLastShoot = 0f;
                _canShoot = true;
            }
        }
    }

    public float currentAmmo
    {
        get { return _currentAmmo; }
        private set
        {
            if (value < 0f)
                _currentAmmo = 0f;
            else
                _currentAmmo = value;
        }
    }

    public bool canShoot
    {
        get { return _canShoot; }
        private set
        {
            _canShoot = value;
        }
    }

    public void Shoot()
    {
        if (_canShoot && _currentAmmo > 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            GameObject flash = Instantiate(flashFrefab, firePoint.position, firePoint.rotation);

            float randomUntolerance = Random.Range(-angleUntolerance, angleUntolerance);
            Vector3 directionVector = firePoint.right;
            directionVector = Quaternion.Euler(0, 0, randomUntolerance) * directionVector;

            bullet.GetComponent<BulletComponent>().direction = directionVector;
        
            --_currentAmmo;
            _canShoot = false;
        }
    }

    public void Reload()
    {
        _canShoot = false;
        _currentAmmo = maxAmmo;
    }

    public void ReloadEnded()
    {
        _canShoot = true;
    }
}
