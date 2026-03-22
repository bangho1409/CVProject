using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    private SpawnPoint[] spawnPoints;

    void Awake()
    {
        // Get all SpawnPoint components from children
        spawnPoints = GetComponentsInChildren<SpawnPoint>(true);

        // Disable all spawn points at start so they don't auto-run
        foreach (var sp in spawnPoints)
        {
            sp.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        foreach (var sp in spawnPoints)
        {
            sp.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        foreach (var sp in spawnPoints)
        {
            sp.enabled = false;
        }
    }
}
