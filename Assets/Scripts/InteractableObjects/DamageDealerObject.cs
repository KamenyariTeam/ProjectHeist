using Characters;
using Characters.Player;
using UnityEngine;

public class DamageDealerObjectComponent : MonoBehaviour
{
    public float damage = 25f;

    //private float _currentHealth = 100f;
    private HealthComponent _lastHealthComponent;

    private void OnTriggerEnter2D(Collider2D triggeredCollider)
    {
        var healthComponent = triggeredCollider.GetComponent<HealthComponent>();

        if (healthComponent != null && healthComponent != _lastHealthComponent)
        {
            healthComponent.TakeDamage(damage);
            _lastHealthComponent = healthComponent;
            Debug.Log("Took Damage");
        }
    }

    private void OnTriggerExit2D(Collider2D triggeredCollider)
    {
        Debug.Log("Leave");
        _lastHealthComponent = null;
    }
}