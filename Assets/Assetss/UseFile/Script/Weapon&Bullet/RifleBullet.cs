using UnityEngine;

public class RifleBullet : MonoBehaviour
{
    [HideInInspector] public float damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Enemy"))
            return;

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
