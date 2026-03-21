using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CharacterDataManager : MonoBehaviour
{
    public static CharacterDataManager Instance; // Singleton để dễ gọi từ mọi nơi
    public TextAsset characterCsv;

    // Lưu trữ dữ liệu nhân vật theo ID
    private Dictionary<int, CharacterData> characterDatabase = new Dictionary<int, CharacterData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if(characterCsv!=null)
        {
            ReadCSV();
        }
    }

    void ReadCSV()
    {
        string[] lines = characterCsv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Bỏ qua tiêu đề
        {
            string[] row = lines[i].Split(',');
            if (row.Length <= 1) continue;

            CharacterData data = new CharacterData();
            data.id = int.Parse(row[0], CultureInfo.InvariantCulture);
            data.name = row[1];
            data.type = int.Parse(row[2], CultureInfo.InvariantCulture);
            data.hp = float.Parse(row[3], CultureInfo.InvariantCulture);
            data.stamina = float.Parse(row[4], CultureInfo.InvariantCulture);
            data.moveSpeed = float.Parse(row[5], CultureInfo.InvariantCulture);
            data.runSpeed = float.Parse(row[6], CultureInfo.InvariantCulture);
            data.dashSpeed = float.Parse(row[7], CultureInfo.InvariantCulture);
            data.dashDuration = float.Parse(row[8], CultureInfo.InvariantCulture);
            data.runStaminaCost = float.Parse(row[9], CultureInfo.InvariantCulture);
            data.dashStaminaCost = float.Parse(row[10], CultureInfo.InvariantCulture);
            data.recoveryStaminaRate = float.Parse(row[11], CultureInfo.InvariantCulture);
            data.experiencePoints = float.Parse(row[12], CultureInfo.InvariantCulture);

            characterDatabase.Add(data.id, data);
        }
        Debug.Log("DataManager: Đã tải xong " + characterDatabase.Count + " nhân vật.");
    }

    public CharacterData GetCharacterById(int id)
    {
        if (characterDatabase.ContainsKey(id)) return characterDatabase[id];
        return null;
    }
}

[System.Serializable]
public class CharacterData
{
    public int id;
    public string name;
    public int type;
    public float hp;
    public float stamina;
    public float moveSpeed;
    public float runSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float runStaminaCost;
    public float dashStaminaCost;
    public float recoveryStaminaRate;
    public float experiencePoints;
}



