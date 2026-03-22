using UnityEngine;

public class HammerDamageCollider : MonoBehaviour
{
    [HideInInspector] public float damage;
    private int enemyLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap") || collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            Destroy(gameObject);
            return;
        }

        // Check by tag OR by layer to handle enemies whose root is Untagged
        if (!collision.CompareTag("Enemy") && collision.gameObject.layer != enemyLayer)
            return;

        // Try EnemyBat (or any enemy with TakeDamage method)
        EnemyBat enemy = collision.GetComponent<EnemyBat>();
        EnemyCrab enemyCrab = collision.GetComponentInParent<EnemyCrab>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (enemyCrab != null)
        {
            enemyCrab.TakeDamage(damage);
        }
    }
}
