using UnityEngine;

public class CrabAttackZoneCensor : MonoBehaviour
{
    private EnemyCrab parentCrab;

    void Start()
    {
        parentCrab = GetComponentInParent<EnemyCrab>();
        if (parentCrab == null)
        {
            Debug.LogWarning($"{name}: CrabAttackZoneCensor could not find an EnemyCrab in parents.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (parentCrab != null)
            {
                parentCrab.OnPlayerEnterAttackZone();
            }
            else
            {
                Debug.LogWarning($"{name}: Triggered by Player but parentCrab is null.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (parentCrab != null)
            {
                parentCrab.OnPlayerExitAttackZone();
            }
        }
    }
}