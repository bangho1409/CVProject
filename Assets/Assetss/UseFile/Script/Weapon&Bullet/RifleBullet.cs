using UnityEngine;

public class RifleBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    [SerializeField] private LayerMask enemyLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap") || collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            Destroy(gameObject);
            return;
        }

        // Check by tag OR by layer to handle enemies whose root is Untagged
        bool isEnemy = collision.CompareTag("Enemy")
            || ((enemyLayer.value & (1 << collision.gameObject.layer)) != 0);

        if (!isEnemy)
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

        // Destroy bullet on hit
        //Destroy(gameObject);
    }
}
