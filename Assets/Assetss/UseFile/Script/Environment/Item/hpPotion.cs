using UnityEngine;

public class hpPotion : MonoBehaviour
{
    private GameObject player;
    private PlayerCharacter PlayerCharacter;

    private ItemData itemData;

    public int id = 100; // ID của bình thuốc HP, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu
    public string itemName; // Tên của bình thuốc HP, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu
    public float amount; // Số lượng HP mà bình thuốc sẽ hồi phục, có thể được gán từ Inspector hoặc từ một hệ thống quản lý dữ liệu

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
                PlayerCharacter.Heal(amount); // Gọi phương thức Heal trên PlayerCharacter để hồi phục HP
                if (PlayerCharacter.hp > PlayerCharacter.maxHp)
                {
                    PlayerCharacter.hp = PlayerCharacter.maxHp; // Đảm bảo HP không vượt quá maxHp
                }
                Destroy(gameObject); // Hủy đối tượng bình thuốc sau khi sử dụng
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
