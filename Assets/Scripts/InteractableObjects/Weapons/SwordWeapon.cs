using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Characters.Player;
using DataStorage.Generated;
using InteractableObjects.Pickups;
using InteractableObjects.Weapons;
using SaveSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InteractableObjects.Weapons
{

    public abstract class IWeapon : MonoBehaviour
    {
        public abstract void Attack();
        public abstract bool CanAttack { get; }
        public abstract float CurrentCooldown { get; }
        public abstract float TotalCooldown { get; }
    }

    public class SwordWeapon : IWeapon
    {
        [SerializeField] private float _attackTime;
        [SerializeField] private float _attackAngle;
        [SerializeField] private float _totalCooldown;
        [SerializeField] private Transform _weaponTransform;

        private float _cooldown;
        private bool _swingDirection;
        private float _attackTimeLeft;

        public override bool CanAttack { get { return _cooldown <= 0 && _attackTimeLeft <= 0; } }
        public override float CurrentCooldown { get { return _cooldown; } }
        public override float TotalCooldown { get { return _totalCooldown;} }

        public override void Attack()
        {
            _attackTimeLeft = _attackTime;
            _cooldown = _totalCooldown;
            _swingDirection = !_swingDirection;
        }

        protected void Awake()
        {
            _cooldown = 0;
            _attackTimeLeft = 0.0f;
            _swingDirection = false;
        }

        protected void Update()
        {
            if (_attackTimeLeft > 0)
            {
                float rotationSpeed = _attackAngle / _attackTime;
                if (!_swingDirection)
                {
                    rotationSpeed *= -1;
                }

                _weaponTransform.RotateAround(transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
                _attackTimeLeft = Mathf.Max(0.0f, _attackTimeLeft - Time.deltaTime);
                return;
            }

            _cooldown = Mathf.Max(0.0f, _cooldown - Time.deltaTime);
        }
    
    }
}