using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap") || collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            // Destroy bullet on hit
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Enemy"))
            return;

        // Deal damage to enemy but do NOT destroy bullet
        // Bullet continues flying until it hits WallMap (handled by BulletMover)
        EnemyBat enemy = collision.GetComponent<EnemyBat>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        EnemyCrab enemyCrab = collision.GetComponent<EnemyCrab>();
        if (enemyCrab != null)
        {
            enemyCrab.TakeDamage(damage);
        }

        // Destroy bullet on hit
        //Destroy(gameObject);
    }
}
