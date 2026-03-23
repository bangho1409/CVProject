using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [HideInInspector] public float damage;

    private int enemyLayer;

    private void Start()
    {
        enemyLayer = LayerMask.NameToLayer("Enemy");
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

        EnemyBat enemy = collision.GetComponentInParent<EnemyBat>();
        EnemyCrab enemyCrab = collision.GetComponentInParent<EnemyCrab>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        else if (enemyCrab != null)
        {
            enemyCrab.TakeDamage(damage);
        }
    }
}
