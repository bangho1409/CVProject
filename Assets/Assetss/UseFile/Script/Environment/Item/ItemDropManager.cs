using UnityEngine;
using System.Collections.Generic;

public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager Instance;

    // Base item IDs (level 1). Each level up adds +1 to the ID.
    [Header("Base Item IDs (Level 1)")]
    [SerializeField] private int baseHpId = 100;
    [SerializeField] private int baseStaminaId = 200;
    [SerializeField] private int baseExpId = 300;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// Returns the level offset based on the player's current id.
    /// Player starts at id 100, so level offset = player.id - 100.
    /// </summary>
    private int GetLevelOffset()
    {
        if (PlayerCharacter.Instance != null)
            return PlayerCharacter.Instance.id - 100;
        return 0;
    }

    /// <summary>
    /// Called when an enemy dies. Rolls drops based on the player's current level.
    /// Spawns items at the given position.
    /// </summary>
    public void RollDrops(Vector3 dropPosition)
    {
        int offset = GetLevelOffset();

        // All possible drop item base IDs
        int[] baseIds = { baseHpId, baseStaminaId, baseExpId };

        foreach (int baseId in baseIds)
        {
            int itemId = baseId + offset;
            ItemData data = ItemDataManager.Instance.GetItemById(itemId);
            if (data == null) continue;

            // Roll chance (0.0 ~ 1.0)
            float roll = Random.value;
            if (roll <= data.chance)
            {
                SpawnItem(data, dropPosition);
            }
        }
    }

    private void SpawnItem(ItemData data, Vector3 position)
    {
        if (string.IsNullOrEmpty(data.prefabPath)) return;

        string path = data.prefabPath.Replace(".prefab", "");

        int resourcesIndex = path.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            path = path.Substring(resourcesIndex + "Resources/".Length);
        }

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            // Slight random offset so items don't stack
            Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
            GameObject item = Instantiate(prefab, position + offset, Quaternion.identity);

            // Set the item's id so it loads the correct level data from CSV
            SetItemId(item, data.id);
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Sets the id field on the spawned item so it loads the correct level's amount.
    /// </summary>
    private void SetItemId(GameObject item, int id)
    {
        hpPotion hp = item.GetComponent<hpPotion>();
        if (hp != null) { hp.id = id; return; }

        staminaPotion stamina = item.GetComponent<staminaPotion>();
        if (stamina != null) { stamina.id = id; return; }

        expPoint exp = item.GetComponent<expPoint>();
        if (exp != null) { exp.id = id; return; }
    }
}