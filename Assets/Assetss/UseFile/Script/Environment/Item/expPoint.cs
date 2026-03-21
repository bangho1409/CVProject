using UnityEngine;

public class expPoint : MonoBehaviour
{
    private GameObject player;
    private PlayerCharacter PlayerCharacter;

    private ItemData itemData;

    public int id = 300; // ID của bình thuốc Stamina, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu
    public string itemName; // Tên của bình thuốc Stamina, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu
    public float amount; // Số lượng Stamina mà bình thuốc sẽ hồi phục, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu

    private void Start()
    {
        itemData = ItemDataManager.Instance.GetItemById(id);
        if (itemData != null)
        {
            itemName = itemData.itemName;
            amount = itemData.amount;
        }
        else
        {
            Debug.LogWarning($"{itemName}: Item ID {id} not found in ItemDataManager.");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Player"))
            return;

        player = collision.gameObject;
        if (player != null)
        {
            PlayerCharacter = player.GetComponent<PlayerCharacter>();
            if (PlayerCharacter != null)
            {
                PlayerCharacter.GainExperience(amount);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"{name}: Player does not have a PlayerCharacter component.");
            }
        }
        else
        {
            Debug.LogWarning($"{name}: Player reference is not set.");
        }
    }
}
