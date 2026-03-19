using System.Collections.Generic;
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
            data.id = int.Parse(row[0]);
            data.name = row[1];
            data.type = int.Parse(row[2]);
            data.hp = float.Parse(row[3]);
            data.stamina = float.Parse(row[4]);
            data.moveSpeed = float.Parse(row[5]);
            data.runSpeed = float.Parse(row[6]);
            data.dashSpeed = float.Parse(row[7]);
            data.dashDuration = float.Parse(row[8]);
            data.runStaminaCost = float.Parse(row[9]);
            data.dashStaminaCost = float.Parse(row[10]);
            data.recoveryStaminaRate = float.Parse(row[11]);
            data.experiencePoints = float.Parse(row[12]);

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



