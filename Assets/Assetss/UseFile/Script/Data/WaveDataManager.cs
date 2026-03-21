using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class WaveDataManager : MonoBehaviour
{
    public static WaveDataManager Instance;
    public TextAsset waveCsv;
    private Dictionary<int, WaveData> waveDatabase = new Dictionary<int, WaveData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        LoadWaveCSV();
    }

    void LoadWaveCSV()
    {
        string[] lines = waveCsv.text.Trim().Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] row = line.Split(',');

            WaveData data = new WaveData();
            data.waveId = int.Parse(row[0], CultureInfo.InvariantCulture);
            data.waveName = row[1];

            // Monster_ID: "200;201" -> List<int>
            string[] ids = row[2].Split(';');
            foreach (string sId in ids)
            {
                data.monsterIds.Add(int.Parse(sId.Trim(), CultureInfo.InvariantCulture));
            }

            // Total_Spawn: số lượng spawn chung cho mỗi loại monster
            data.totalSpawn = int.Parse(row[3].Trim(), CultureInfo.InvariantCulture);

            // Spawn_Interval: "1.5;0.5" -> mỗi monster có interval riêng
            string[] intervals = row[4].Split(';');
            foreach (string sTime in intervals)
            {
                data.spawnIntervals.Add(float.Parse(sTime.Trim(), CultureInfo.InvariantCulture));
            }

            waveDatabase.Add(data.waveId, data);
        }
        Debug.Log("Đã tải xong " + waveDatabase.Count + " waves.");
    }

    public WaveData GetWaveById(int id)
    {
        if (waveDatabase.ContainsKey(id)) return waveDatabase[id];
        return null;
    }

    public List<WaveData> GetAllWaves()
    {
        return new List<WaveData>(waveDatabase.Values);
    }
}


[System.Serializable]
public class WaveData
{
    public int waveId;
    public string waveName;
    public List<int> monsterIds = new List<int>();       // Danh sách ID quái vật sẽ xuất hiện
    public int totalSpawn;                                // Tổng số spawn cho MỖI loại monster
    public List<float> spawnIntervals = new List<float>(); // Interval riêng cho mỗi loại monster
}

