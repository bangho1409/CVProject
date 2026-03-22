using UnityEngine;

public class BoomAttack : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Boom GameObject child of Player (contains SpriteRenderer).")]
    [SerializeField] private GameObject boomObject;

    [Tooltip("The throw point Transform inside the Boom weapon.")]
    [SerializeField] private Transform throwPoint;

    [Tooltip("Player Animator (same one used by PlayerCharacterController).")]
    [SerializeField] private Animator playerAnimator;

    [Tooltip("Player SpriteRenderer (to read flipX for facing direction).")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Weapon Switcher")]
    [Tooltip("WeaponSwitcher to temporarily disable gun/rifle during bomb throw.")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("Bomb Data")]
    [Tooltip("BulletBase CSV ID for Bomb.")]
    [SerializeField] private int bombDataId = 400;

    // Data from CSV
    private BaseBulletData bombData;

    // Bomb prefab loaded from Resources
    private GameObject bombPrefab;

    // Attack state
    private bool isAttacking = false;
    private bool canAttack = true;
    private float cooldownTimer = 0f;

    // Store original local position of boomObject for proper flipping
    private float boomOriginalLocalX;

    void Start()
    {
        // Load bomb data from CSV
        if (BaseBulletDataManager.Instance != null)
        {
            bombData = BaseBulletDataManager.Instance.GetBulletById(bombDataId);
            if (bombData != null)
            {
                string path = bombData.prefabPath.Replace(".prefab", "");
                bombPrefab = Resources.Load<GameObject>(path);
                if (bombPrefab == null)
                {
                    Debug.LogWarning($"BoomAttack: Could not load prefab at '{path}'.");
                }
            }
            else
            {
                Debug.LogWarning($"BoomAttack: No BaseBulletData found for id {bombDataId}.");
            }
        }
        else
        {
            Debug.LogWarning("BoomAttack: BaseBulletDataManager.Instance is null.");
        }

        // Cache the original local X position of the boom object
        if (boomObject != null)
        {
            boomOriginalLocalX = Mathf.Abs(boomObject.transform.localPosition.x);
            boomObject.SetActive(false);
        }
    }

    void Update()
    {
        // Update cooldown timer
        if (!canAttack)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canAttack = true;
            }
        }

        // Flip boom object to match player facing direction
        FlipBoomWithPlayer();
    }

    /// <summary>
    /// Called by UI Button "Bomb". Single press like Hammer, not holding.
    /// </summary>
    public void OnBombAttack()
    {
        if (!canAttack || isAttacking || bombData == null || bombPrefab == null)
            return;

        isAttacking = true;
        canAttack = false;

        // Temporarily disable gun/rifle
        if (weaponSwitcher != null)
        {
            weaponSwitcher.DisableForHammer();
        }

        // Show boom visual briefly
        if (boomObject != null)
        {
            boomObject.SetActive(true);
        }

        // Trigger player bomb throw animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("isBomb");
        }

        // Spawn projectile and immediately end attack visual
        SpawnBombBullet();
        EndAttack();
    }

    private void SpawnBombBullet()
    {
        if (bombPrefab == null || throwPoint == null) return;

        Vector3 startPos = throwPoint.position;

        // Find nearest alive enemy for shoot direction
        Transform nearestEnemy = FindNearestAliveEnemy(transform.position);
        Vector2 shootDir = GetShootDirection(startPos, nearestEnemy);

        // Spawn bomb at throw point
        GameObject bombObj = Instantiate(bombPrefab, startPos, Quaternion.identity);

        BoomBullet bullet = bombObj.GetComponent<BoomBullet>();
        if (bullet == null) bullet = bombObj.AddComponent<BoomBullet>();

        // Gán dữ liệu từ CSV
        bullet.damage = bombData.damage;
        bullet.explosionRadius = bombData.radius;
        bullet.boomData = bombData;

        // Use BulletMover for movement (same as Gun/Rifle)
        BulletMover mover = bombObj.GetComponent<BulletMover>();
        if (mover == null)
            mover = bombObj.AddComponent<BulletMover>();
        mover.Initialize(shootDir, bombData.speed);
    }

    private void EndAttack()
    {
        isAttacking = false;

        // Immediately hide boom visual
        if (boomObject != null)
        {
            boomObject.SetActive(false);
        }

        // Start cooldown
        cooldownTimer = bombData != null ? bombData.cooldown : 1f;

        // Re-enable gun/rifle
        if (weaponSwitcher != null)
        {
            weaponSwitcher.ReEnableAfterHammer();
        }
    }

    // ─── AUTO-AIM ───────────────────────────────────────────────

    private Transform FindNearestAliveEnemy(Vector3 origin)
    {
        Transform nearest = null;
        float closestDist = Mathf.Infinity;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (!enemy.activeInHierarchy) continue;

            EnemyBat enemyBat = enemy.GetComponent<EnemyBat>();
            if (enemyBat != null && enemyBat.hp <= 0) continue;

            float dist = Vector2.Distance(origin, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private Vector2 GetShootDirection(Vector3 fromPos, Transform nearestEnemy)
    {
        if (nearestEnemy != null)
        {
            return ((Vector2)(nearestEnemy.position - fromPos)).normalized;
        }
        return GetFacingDirection();
    }

    // ─── FLIP ───────────────────────────────────────────────────

    private void FlipBoomWithPlayer()
    {
        if (boomObject == null || playerSpriteRenderer == null) return;

        Vector3 boomScale = boomObject.transform.localScale;
        Vector3 boomPos = boomObject.transform.localPosition;

        if (playerSpriteRenderer.flipX)
        {
            // Player facing right (flipX = true)
            boomScale.x = -Mathf.Abs(boomScale.x);
            boomPos.x = boomOriginalLocalX;
        }
        else
        {
            // Player facing left (flipX = false)
            boomScale.x = Mathf.Abs(boomScale.x);
            boomPos.x = -boomOriginalLocalX;
        }

        boomObject.transform.localScale = boomScale;
        boomObject.transform.localPosition = boomPos;
    }

    private Vector2 GetFacingDirection()
    {
        if (playerSpriteRenderer != null && playerSpriteRenderer.flipX)
        {
            return Vector2.right;
        }
        return Vector2.left;
    }
}
