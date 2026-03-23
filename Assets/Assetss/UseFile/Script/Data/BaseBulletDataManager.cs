using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class BaseBulletDataManager : MonoBehaviour
{
    public static BaseBulletDataManager Instance;
    public TextAsset bulletCSV;

    private Dictionary<int, BaseBulletData> baseBulletData = new Dictionary<int, BaseBulletData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (bulletCSV != null)
        {
            ReadCSV();
        }
    }

    void ReadCSV()
    {
        string[] lines = bulletCSV.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length <= 1) continue;
            BaseBulletData data = new BaseBulletData();
            data.id = int.Parse(row[0], CultureInfo.InvariantCulture);
            data.bulletName = row[1];
            data.damage = float.Parse(row[2], CultureInfo.InvariantCulture);
            data.speed = float.Parse(row[3], CultureInfo.InvariantCulture);
            data.radius = float.Parse(row[4], CultureInfo.InvariantCulture);
            data.cooldown = float.Parse(row[5], CultureInfo.InvariantCulture);
            data.delayTime = float.Parse(row[6], CultureInfo.InvariantCulture);
            data.staminaCost = float.Parse(row[7], CultureInfo.InvariantCulture);
            data.prefabPath = row[8].Trim();
            data.fxPath = row[9].Trim();
            baseBulletData.Add(data.id, data);
        }
    }

    public BaseBulletData GetBulletById(int id)
    {
        if (baseBulletData.ContainsKey(id))
        {
            return baseBulletData[id];
        }
        return null;
    }
}

[System.Serializable]
public class BaseBulletData
{
    public int id;
    public string bulletName;
    public float damage;
    public float speed;
    public float radius;
    public float cooldown;
    public float delayTime;
    public float staminaCost;
    public string prefabPath;
    public string fxPath;
}

