using UnityEngine;

public class HammerDamageCollider : MonoBehaviour
{
    [HideInInspector] public float damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        // Try EnemyBat (or any enemy with TakeDamage method)
        EnemyBat enemy = collision.GetComponent<EnemyBat>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"HammerDamageCollider: Hit {collision.name} for {damage} damage.");
        }

        EnemyCrab enemyCrab = collision.GetComponent<EnemyCrab>();
        if (enemyCrab != null)
        {
            enemyCrab.TakeDamage(damage);
            Debug.Log($"HammerDamageCollider: Hit {collision.name} for {damage} damage.");
        }
    }
}
