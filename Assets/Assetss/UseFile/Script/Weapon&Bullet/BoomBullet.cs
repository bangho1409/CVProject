using UnityEngine;

public class BoomBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float explosionRadius;
    [HideInInspector] public BaseBulletData boomData;
    [SerializeField] private LayerMask enemyLayer;

    private GameObject fxPrefab;

    private void Start()
    {
        if (boomData == null || string.IsNullOrEmpty(boomData.fxPath))
        {
            Debug.LogWarning("BoomBullet: boomData or fxPath is null. Was boomData assigned from BoomAttack?");
            return;
        }

        string path = boomData.fxPath.Replace(".prefab", "");
        fxPrefab = Resources.Load<GameObject>(path);
        if (fxPrefab == null)
        {
            Debug.LogWarning($"BoomBullet: Could not load FX prefab at '{path}'.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check by tag OR by layer to handle enemies whose root object is Untagged
        if (collision.CompareTag("WallMap")
            || collision.CompareTag("Enemy")
            || ((enemyLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // Search on the collider's own object first, then walk up to root
            EnemyBat enemy = enemyCollider.GetComponentInParent<EnemyBat>();
            EnemyCrab enemyCrab = enemyCollider.GetComponentInParent<EnemyCrab>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else if (enemyCrab != null)
            {
                enemyCrab.TakeDamage(damage);
            }
        }
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        if (fxPrefab != null)
        {
            Instantiate(fxPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}