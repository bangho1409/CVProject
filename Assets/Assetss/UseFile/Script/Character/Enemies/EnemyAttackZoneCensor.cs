using UnityEngine;

public class EnemyAttackZoneCensor : MonoBehaviour
{
    private EnemyBat parentBat;

    void Start()
    {
        // Lấy reference tới EnemyBat ở parent (hoặc ancestor)
        parentBat = GetComponentInParent<EnemyBat>();
        if (parentBat == null)
        {
            Debug.LogWarning($"{name}: EnemyAttackZoneCensor could not find an EnemyBat in parents.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (parentBat != null)
            {
                // Inform the bat; let the bat decide whether to attack (cooldown/stamina)
                parentBat.OnPlayerEnterAttackZone();
            }
            else
            {
                Debug.LogWarning($"{name}: Triggered by Player but parentBat is null.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (parentBat != null)
            {
                parentBat.OnPlayerExitAttackZone();
            }
        }
    }
}
