using UnityEngine;

public class BulletMover : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private bool initialized = false;

    /// <summary>
    /// Initialize the bullet's movement direction and speed.
    /// Bullet flies in a straight line until it hits a WallMap collider.
    /// </summary>
    public void Initialize(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        // Move the bullet in a straight line
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only destroy bullet when hitting the map border (tag "WallMap", layer "Map")
        if (collision.CompareTag("WallMap"))
        {
            Destroy(gameObject);
        }
    }
}