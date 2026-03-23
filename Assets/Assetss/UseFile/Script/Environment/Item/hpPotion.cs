using UnityEngine;

public class hpPotion : MonoBehaviour
{
    private ItemData itemData;

    public int id = 100;
    public string itemName;
    public float amount;

    private void Start()
    {
        itemData = ItemDataManager.Instance.GetItemById(id);
        if (itemData != null)
        {
            itemName = itemData.itemName;
            amount = itemData.amount;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WallMap"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Player")) return;

        PlayerCharacter player = collision.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.Heal(amount);
            Destroy(gameObject);
        }
    }
}
