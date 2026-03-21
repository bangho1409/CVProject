using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class EnemiesDataManager : MonoBehaviour
{
    public static EnemiesDataManager Instance; // Singleton để dễ gọi từ mọi nơi
    public TextAsset enemyCSV;

    // Lưu trữ dữ liệu nhân vật theo ID
    private Dictionary<int, EnemyData> enemyDatabase = new Dictionary<int, EnemyData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if (enemyCSV != null)
        {
            ReadCSV();
        }
    }

    void ReadCSV()
    {
        string[] lines = enemyCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Bỏ qua tiêu đề
        {
            string[] row = lines[i].Split(',');
            if (row.Length <= 1) continue;

            EnemyData data = new EnemyData();
            data.id = int.Parse(row[0], CultureInfo.InvariantCulture);
            data.name = row[1];
            data.type = int.Parse(row[2], CultureInfo.InvariantCulture);
            data.hp = float.Parse(row[3], CultureInfo.InvariantCulture);
            data.stamina = float.Parse(row[4], CultureInfo.InvariantCulture);
            data.skillStaminaCost = float.Parse(row[5], CultureInfo.InvariantCulture);
            data.touchAttackDamage = float.Parse(row[6], CultureInfo.InvariantCulture);
            data.moveSpeed = float.Parse(row[7], CultureInfo.InvariantCulture);
            data.runSpeed = float.Parse(row[8], CultureInfo.InvariantCulture);
            data.dashSpeed = float.Parse(row[9], CultureInfo.InvariantCulture);
            data.dashDuration = float.Parse(row[10], CultureInfo.InvariantCulture);
            data.recoveryStaminaRate = float.Parse(row[11], CultureInfo.InvariantCulture);
            data.skillDamage = float.Parse(row[12], CultureInfo.InvariantCulture);
            data.skillCooldown = float.Parse(row[13], CultureInfo.InvariantCulture);
            data.retreatDistance = float.Parse(row[14], CultureInfo.InvariantCulture);
            data.PrefabPath = row[15].Trim();
            data.itemDropPath = row[16].Trim();

            enemyDatabase.Add(data.id, data);
        }
        Debug.Log("DataManager: Đã tải xong " + enemyDatabase.Count + " quái.");

    }

    public EnemyData GetCharacterById(int id)
    {
        if (enemyDatabase.ContainsKey(id)) return enemyDatabase[id];
        return null;
    }

    //get all character data as list
    public List<EnemyData> GetAllCharacters()
    {
        return new List<EnemyData>(enemyDatabase.Values);
    }
}
public class EnemyData
{
    public int id;
    public string name;
    public int type;
    public float hp;
    public float stamina;
    public float skillStaminaCost;
    public float touchAttackDamage;
    public float moveSpeed;
    public float runSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float recoveryStaminaRate;
    public float skillDamage;
    public float skillCooldown;
    public float retreatDistance;
    public string PrefabPath;
    public string itemDropPath;
}

