using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Wave Settings")]
    public int startWaveId = 100; // WaveID đầu tiên

    private WaveData currentWave;
    private int totalSpawned = 0;
    private int totalExpected = 0;
    private bool isInfiniteWave = false;

    void Start()
    {
        StartWave(startWaveId);
    }

    // Bắt đầu wave theo WaveID
    public void StartWave(int waveId)
    {
        currentWave = WaveDataManager.Instance.GetWaveById(waveId);
        if (currentWave == null)
        {
            Debug.LogWarning("Không tìm thấy WaveID: " + waveId);
            return;
        }

        Debug.Log("Bắt đầu Wave: " + currentWave.waveName);

        totalSpawned = 0;
        isInfiniteWave = currentWave.totalSpawn == -1;
        totalExpected = isInfiniteWave ? -1 : currentWave.monsterIds.Count * currentWave.totalSpawn;

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

    IEnumerator SpawnMonsterRoutine(int monsterId, int count, float interval)
    {
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

                if (totalSpawned >= totalExpected)
                {
                    Debug.Log("Wave hoàn thành! Tổng đã spawn: " + totalSpawned);
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }

    void SpawnMonster(int id)
    {
        EnemyData enemyData = EnemiesDataManager.Instance.GetCharacterById(id);
        if (enemyData == null)
        {
            Debug.LogWarning("Không tìm thấy EnemyData với ID: " + id);
            return;
        }

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
            Debug.Log("Spawned: " + enemyData.name + " (ID: " + id + ")");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Prefab tại: " + path + " (gốc: " + enemyData.PrefabPath + ")");
        }
    }
}