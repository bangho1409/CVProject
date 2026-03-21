using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public float amount;
    public float chance;
    public string prefabPath;
}

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;
    public TextAsset csvFile;

    public Dictionary<int, ItemData> itemList = new Dictionary<int, ItemData>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if (csvFile != null)
        {
            ReadCSV();
        }
    }
    public ItemData GetItemById(int id)
    {
        if (itemList.ContainsKey(id))
        {
            return itemList[id];
        }
        return null;
    }
    void ReadCSV()
    {
        string[] data = csvFile.text.Split(new char[] { '\n' });

        // i = 1 để bỏ dòng tiêu đề
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(new char[] { ',' });
            if (row.Length <= 1) continue;
            ItemData item = new ItemData();
            int.TryParse(row[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out item.id);
            item.itemName = row[1].Trim();
            float.TryParse(row[2], NumberStyles.Float, CultureInfo.InvariantCulture, out item.amount);
            float.TryParse(row[3], NumberStyles.Float, CultureInfo.InvariantCulture, out item.chance);
            item.prefabPath = row[4].Trim();
            itemList.Add(item.id, item);
        }

        Debug.Log("Đã nạp thành công " + itemList.Count + " vật phẩm.");
    }
}
