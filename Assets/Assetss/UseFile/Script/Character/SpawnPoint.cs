using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Wave Settings")]
    public int startWaveId = 100; // WaveID đầu tiên

    private WaveData currentWave;
    public int currentWaveId { get; private set; }
    private int totalSpawned = 0;
    private int totalExpected = 0;
    private bool isInfiniteWave = false;
    private bool isSpawning = false;

    void Start()
    {
        currentWaveId = startWaveId;
        StartWave(currentWaveId);
    }

    /// <summary>
    /// Called when the player levels up.
    /// Stops the current wave and starts the next wave (waveId + 1).
    /// If no next wave exists, restarts the current wave.
    /// </summary>
    public void OnPlayerLevelUp()
    {
        StopAllCoroutines();
        isSpawning = false;

        WaveData nextWave = WaveDataManager.Instance.GetWaveById(currentWaveId + 1);
        if (nextWave != null)
        {
            currentWaveId++;
            StartWave(currentWaveId);
        }
    }

    // Bắt đầu wave theo WaveID
    public void StartWave(int waveId)
    {
        if (!gameObject.activeInHierarchy) return;

        if (isSpawning)
        {
            StopAllCoroutines();
            isSpawning = false;
        }

        currentWave = WaveDataManager.Instance.GetWaveById(waveId);
        if (currentWave == null) return;

        currentWaveId = waveId;
        totalSpawned = 0;
        isInfiniteWave = currentWave.totalSpawn == -1;
        totalExpected = isInfiniteWave ? -1 : currentWave.monsterIds.Count * currentWave.totalSpawn;

        isSpawning = true;

        // Mỗi loại monster chạy coroutine spawn riêng (đồng thời)
        for (int i = 0; i < currentWave.monsterIds.Count; i++)
        {
            int monsterId = currentWave.monsterIds[i];
            int spawnCount = currentWave.totalSpawn;
            float interval = (i < currentWave.spawnIntervals.Count)
                ? currentWave.spawnIntervals[i]
                : 1f; // fallback nếu thiếu interval

            StartCoroutine(SpawnMonsterRoutine(monsterId, spawnCount, interval));
        }
    }

    /// <summary>
    /// Dừng spawn, được gọi từ SpawnTrigger.
    /// </summary>
    public void PauseSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// Tiếp tục spawn wave hiện tại, được gọi từ SpawnTrigger.
    /// </summary>
    public void ResumeSpawning()
    {
        if (!gameObject.activeInHierarchy) return;
        if (isSpawning) return;

        StartWave(currentWaveId);
    }

    IEnumerator SpawnMonsterRoutine(int monsterId, int count, float interval)
    {
        // Chờ 1 interval trước khi spawn con đầu tiên,
        // tránh spawn ngay lập tức trong cùng frame
        yield return new WaitForSeconds(interval);

        // count == -1 means infinite spawning
        if (count == -1)
        {
            while (true)
            {
                SpawnMonster(monsterId);
                totalSpawned++;
                yield return new WaitForSeconds(interval);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                SpawnMonster(monsterId);
                totalSpawned++;

                if (i < count - 1)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        isSpawning = false;
    }

    void SpawnMonster(int id)
    {
        EnemyData enemyData = EnemiesDataManager.Instance.GetCharacterById(id);
        if (enemyData == null) return;

        // Chuẩn hóa PrefabPath cho Resources.Load:
        // - Bỏ prefix "Assets/.../Resources/" nếu có
        // - Bỏ đuôi ".prefab" nếu có
        string path = enemyData.PrefabPath;

        int resourcesIndex = path.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            path = path.Substring(resourcesIndex + "Resources/".Length);
        }

        if (path.EndsWith(".prefab"))
        {
            path = path.Substring(0, path.Length - ".prefab".Length);
        }

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}