using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    private SpawnPoint[] spawnPoints;
    [SerializeField] private int limitEnemies = 200;

    private bool isPaused = false;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<SpawnPoint>(true);
    }

    void Update()
    {
        int currentEnemies = GetAliveEnemyCount();

        if (!isPaused && currentEnemies >= limitEnemies)
        {
            isPaused = true;
            foreach (var sp in spawnPoints)
            {
                sp.PauseSpawning();
            }
        }
        else if (isPaused && currentEnemies < limitEnemies)
        {
            isPaused = false;
            foreach (var sp in spawnPoints)
            {
                sp.ResumeSpawning();
            }
        }
    }

    private int GetAliveEnemyCount()
    {
        int count = 0;

        EnemyBat[] bats = FindObjectsByType<EnemyBat>(FindObjectsSortMode.None);
        foreach (var bat in bats)
        {
            if (!bat.isDead) count++;
        }

        EnemyCrab[] crabs = FindObjectsByType<EnemyCrab>(FindObjectsSortMode.None);
        foreach (var crab in crabs)
        {
            if (!crab.isDead) count++;
        }

        return count;
    }
}
