using UnityEngine;

public class staminaPotion : MonoBehaviour
{
    private ItemData itemData;

    public int id = 200;
    public string itemName;
    public float amount;

    private float destroyDelay = 360f;
    private float timer;

    private void Start()
    {
        itemData = ItemDataManager.Instance.GetItemById(id);
        if (itemData != null)
        {
            itemName = itemData.itemName;
            amount = itemData.amount;
        }
        timer = destroyDelay;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
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
            player.RecoverStamina(amount);
            Destroy(gameObject);
        }
    }
}
