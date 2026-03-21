using Unity.VisualScripting;
using UnityEngine;

public class dropItem : MonoBehaviour
{
    [Header("Child Item References")]
    [SerializeField] private GameObject hpPotionObject;
    [SerializeField] private GameObject pointObject;
    [SerializeField] private GameObject staminaPotionObject;

    void Start()
    {  
        if (ItemDataManager.Instance == null)
        {
            Debug.LogWarning($"{name}: ItemDataManager.Instance is null.");
            return;
        }

        // Roll chance for each item: HP Potion (100), Stamina Potion (200), Exp Point (300)
        RollItem(hpPotionObject, 100);
        RollItem(staminaPotionObject, 200);
        RollItem(pointObject, 300);

        // If no item survived the roll, destroy the entire drop
        bool anyActive = (hpPotionObject != null && hpPotionObject.activeSelf)
                      || (pointObject != null && pointObject.activeSelf)
                      || (staminaPotionObject != null && staminaPotionObject.activeSelf);

        if (!anyActive)
        {
            Destroy(gameObject);
        }
    }

    private void RollItem(GameObject itemObject, int itemId)
    {
        if (itemObject == null) return;

        ItemData data = ItemDataManager.Instance.GetItemById(itemId);
        if (data == null)
        {
            Debug.LogWarning($"{name}: Item ID {itemId} not found.");
            Destroy(itemObject);
            return;
        }

        // Random.value returns [0.0, 1.0) — compare with chance (e.g. 0.05 = 5%)
        float roll = Random.value;
        if (roll <= data.chance)
        {
            itemObject.SetActive(true);
            Debug.Log($"{name}: Dropped {data.itemName} (roll: {roll:F3} <= chance: {data.chance})");
        }
        else
        {
            Destroy(itemObject);
        }
    }
} 
