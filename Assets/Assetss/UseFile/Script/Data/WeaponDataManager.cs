using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class WeaponData
{
    public int id;
    public string weaponName;
    public string ammoType;
    public int level;
    public float damage;
    public float cooldown;
    public string spritePath;
    public string prefabPath;
}

public class WeaponDataManager : MonoBehaviour
{
    public static WeaponDataManager Instance;
    public TextAsset csvFile;

    private Dictionary<int, WeaponData> weaponList = new Dictionary<int, WeaponData>();


    void Start()
    {
        if (csvFile != null)
        {
            ReadCSV();
        }
    }
    void ReadCSV()
    {
        string[] data = csvFile.text.Split(new char[] { '\n' });

        // i = 1 để bỏ dòng tiêu đề
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(new char[] { ',' });
            if (row.Length <= 1) continue;
            WeaponData w = new WeaponData();
            int.TryParse(row[0], out w.id);
            w.weaponName = row[1].Trim();
            w.ammoType = row[2].Trim();
            int.TryParse(row[3], out w.level);
            w.spritePath = row[4].Trim();
            w.prefabPath = row[5].Trim();
            weaponList.Add(w.id, w);
        }

        Debug.Log("Đã nạp thành công " + weaponList.Count + " vũ khí.");
    }

    public bool SpriteExists(string spritePath)
    {
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        return sprite != null;
    }

    // Hàm hỗ trợ tìm vũ khí theo tên
    public WeaponData GetWeaponByName(string name)
    {
        foreach (var weapon in weaponList.Values)
        {
            if (weapon.weaponName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return weapon;
            }
        }
        return null;
    }
}