using UnityEngine;

public class GunAttack : MonoBehaviour
{
    public enum WeaponType { Gun, Rifle }

    [Header("Weapon Type")]
    [SerializeField] private WeaponType weaponType = WeaponType.Gun;

    [Header("References")]
    [Tooltip("The weapon GameObject child of Player (Gun or Rifle).")]
    [SerializeField] private GameObject weaponObject;

    [Tooltip("The shooting point Transform inside the weapon.")]
    [SerializeField] private Transform shootingPoint;

    [Tooltip("MuzzleFlash GameObject inside the weapon.")]
    [SerializeField] private GameObject muzzleFlash;

    [Tooltip("Player Animator (same one used by PlayerCharacterController).")]
    [SerializeField] private Animator playerAnimator;

    [Tooltip("Player SpriteRenderer (to read flipX for facing direction).")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Auto-Aim Settings")]
    [Tooltip("Maximum distance to search for enemies.")]
    [SerializeField] private float autoAimRange = 7f;

    // Data from CSV (BulletBase: Gun = 200, Rifle = 300 for example)
    private BaseBulletData bulletData;

    // Bullet prefab loaded from Resources
    private GameObject bulletPrefab;

    // Shooting state
    private bool isActive = false;
    private float cooldownTimer = 0f;
    private bool canShoot = true;
    private float delayTimer = 0f;
    private bool waitingForDelay = false;

    // Temporarily disabled by hammer
    private bool disabledByHammer = false;

    // Muzzle flash timer
    private float muzzleFlashTimer = 0f;
    private const float MUZZLE_FLASH_DURATION = 0.08f;

    // Cached reference for auto-aim
    private Transform nearestEnemy;

    // Track current facing
    private bool isFacingRight = false;

    // Bullet data IDs (configure in Inspector or keep defaults)
    [Header("Bullet Data")]
    [Tooltip("BulletBase CSV ID for this weapon's bullet.")]
    [SerializeField] private int bulletDataId = 200; // Gun=200, Rifle=300

    void Start()
    {
        // Load bullet data from CSV
        if (BaseBulletDataManager.Instance != null)
        {
            bulletData = BaseBulletDataManager.Instance.GetBulletById(bulletDataId);
            if (bulletData != null)
            {
                // Load the bullet prefab from Resources
                string path = bulletData.prefabPath.Replace(".prefab", "");
                bulletPrefab = Resources.Load<GameObject>(path);
                if (bulletPrefab == null)
                {
                    Debug.LogWarning($"GunAttack ({weaponType}): Could not load prefab at '{path}'.");
                }
            }
            else
            {
                Debug.LogWarning($"GunAttack ({weaponType}): No BaseBulletData found for id {bulletDataId}.");
            }
        }
        else
        {
            Debug.LogWarning($"GunAttack ({weaponType}): BaseBulletDataManager.Instance is null.");
        }

        // Initially disable weapon visual
        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActive || disabledByHammer) return;

        // Update cooldown timer (only counts down AFTER bullet has been fired)
        if (!canShoot)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canShoot = true;
            }
        }

        // Handle delay (pre-fire delay before the bullet spawns)
        if (waitingForDelay)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                waitingForDelay = false;
                FireBullet();

                // Cooldown starts NOW, after the bullet is actually fired
                cooldownTimer = bulletData.cooldown;
            }
        }

        // Find nearest enemy for auto-face and bullet direction
        FindNearestEnemy();

        // Auto-attack: shoot automatically when enemy is in range
        if (nearestEnemy != null && canShoot && !waitingForDelay)
        {
            StartShot();
        }

        // Auto-face player toward nearest enemy
        AutoFaceNearestEnemy();

        // Update facing AFTER auto-face
        isFacingRight = playerSpriteRenderer != null && playerSpriteRenderer.flipX;

        // Muzzle flash timer
        if (muzzleFlash != null && muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f)
            {
                HideMuzzleFlash();
            }
        }
    }

    void LateUpdate()
    {
        if (!isActive || disabledByHammer) return;

        FlipWeaponWithPlayer();
    }

    // ─── PUBLIC METHODS ─────────────────────────────────────────

    public void ActivateWeapon()
    {
        isActive = true;
        disabledByHammer = false;

        if (weaponObject != null)
        {
            weaponObject.SetActive(true);
        }

        SetAnimatorWeaponState(true);
    }

    public void DeactivateWeapon()
    {
        isActive = false;
        waitingForDelay = false;
        disabledByHammer = false;

        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }

        HideMuzzleFlash();
        SetAnimatorWeaponState(false);
    }

    /// <summary>
    /// Called by WeaponSwitcher at the START of hammer swing.
    /// Hides weapon and clears animator bools. No timer — HammerAttack controls re-enable.
    /// </summary>
    public void DisableForHammer()
    {
        if (!isActive) return;

        disabledByHammer = true;
        waitingForDelay = false;

        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }

        HideMuzzleFlash();
        SetAnimatorWeaponState(false);
    }

    /// <summary>
    /// Called by WeaponSwitcher when HammerAttack.EndAttack() fires.
    /// Re-enables weapon and restores animator bools.
    /// </summary>
    public void ReEnableAfterHammer()
    {
        disabledByHammer = false;

        if (!isActive) return;

        if (weaponObject != null)
        {
            weaponObject.SetActive(true);
        }

        SetAnimatorWeaponState(true);
    }

    /// <summary>
    /// Upgrade bullet stats by loading the next bullet data (id + 1) from CSV.
    /// Called when the player levels up.
    /// </summary>
    public void LevelUpBullet()
    {
        if (BaseBulletDataManager.Instance == null || bulletData == null) return;

        BaseBulletData nextData = BaseBulletDataManager.Instance.GetBulletById(bulletData.id + 1);
        if (nextData != null)
        {
            bulletData = nextData;
            bulletDataId = nextData.id;

            // Reload prefab if the next level uses a different one
            string path = bulletData.prefabPath.Replace(".prefab", "");
            GameObject newPrefab = Resources.Load<GameObject>(path);
            if (newPrefab != null)
            {
                bulletPrefab = newPrefab;
            }
            else
            {
                Debug.LogWarning($"GunAttack ({weaponType}): Could not load prefab at '{path}' for bullet id {bulletData.id}.");
            }

            Debug.Log($"GunAttack ({weaponType}): Bullet upgraded to {bulletData.bulletName} (ID: {bulletData.id})");
        }
        else
        {
            Debug.LogWarning($"GunAttack ({weaponType}): No next bullet data found for id {bulletData.id + 1}.");
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    // ─── PRIVATE HELPERS ────────────────────────────────────────

    private void SetAnimatorWeaponState(bool active)
    {
        if (playerAnimator == null) return;

        if (active)
        {
            playerAnimator.SetBool("isHolding", true);

            if (weaponType == WeaponType.Gun)
            {
                playerAnimator.SetBool("isGun", true);
                playerAnimator.SetBool("isRifle", false);
            }
            else
            {
                playerAnimator.SetBool("isGun", false);
                playerAnimator.SetBool("isRifle", true);
            }
        }
        else
        {
            playerAnimator.SetBool("isHolding", false);
            playerAnimator.SetBool("isGun", false);
            playerAnimator.SetBool("isRifle", false);
        }
    }

    // ─── SHOOTING LOGIC ─────────────────────────────────────────

    private void StartShot()
    {
        if (bulletData == null || !canShoot) return;

        // Check if player has enough stamina
        PlayerCharacter player = PlayerCharacter.Instance;
        if (player != null && bulletData.staminaCost > 0)
        {
            if (player.stamina < bulletData.staminaCost) return;
            player.ConsumeStamina(bulletData.staminaCost);
        }

        canShoot = false;
        waitingForDelay = true;
        delayTimer = bulletData.delayTime;
    }

    private void FireBullet()
    {
        if (bulletPrefab == null || shootingPoint == null) return;

        ShowMuzzleFlash();

        Vector2 shootDirection = GetShootDirection();

        GameObject bulletObj = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);

        bulletObj.transform.localScale = new Vector3(1f, 1f, 1f);

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (weaponType == WeaponType.Gun)
        {
            GunBullet gunBullet = bulletObj.GetComponent<GunBullet>();
            if (gunBullet == null)
                gunBullet = bulletObj.AddComponent<GunBullet>();
            gunBullet.damage = bulletData.damage;
        }
        else
        {
            RifleBullet rifleBullet = bulletObj.GetComponent<RifleBullet>();
            if (rifleBullet == null)
                rifleBullet = bulletObj.AddComponent<RifleBullet>();
            rifleBullet.damage = bulletData.damage;
        }

        BulletMover mover = bulletObj.GetComponent<BulletMover>();
        if (mover == null)
            mover = bulletObj.AddComponent<BulletMover>();
        mover.Initialize(shootDirection, bulletData.speed);
    }

    // ─── MUZZLE FLASH ───────────────────────────────────────────

    private void ShowMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.SetActive(true);
        SpriteRenderer mfRenderer = muzzleFlash.GetComponent<SpriteRenderer>();
        if (mfRenderer != null)
        {
            mfRenderer.enabled = true;
        }

        muzzleFlashTimer = MUZZLE_FLASH_DURATION;
    }

    private void HideMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.SetActive(false);
        muzzleFlashTimer = 0f;
    }

    // ─── AUTO-FACE & DIRECTION ──────────────────────────────────

    private void FindNearestEnemy()
    {
        nearestEnemy = null;
        float closestDist = autoAimRange;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearestEnemy = enemy.transform;
            }
        }
    }

    private void AutoFaceNearestEnemy()
    {
        if (playerSpriteRenderer == null || nearestEnemy == null) return;

        float dirX = nearestEnemy.position.x - transform.position.x;

        if (dirX > 0.05f)
        {
            playerSpriteRenderer.flipX = true;
        }
        else if (dirX < -0.05f)
        {
            playerSpriteRenderer.flipX = false;
        }
    }

    private Vector2 GetShootDirection()
    {
        if (nearestEnemy != null)
        {
            return (nearestEnemy.position - shootingPoint.position).normalized;
        }

        return GetFacingDirection();
    }

    private Vector2 GetFacingDirection()
    {
        if (isFacingRight)
        {
            return Vector2.right;
        }
        return Vector2.left;
    }

    // ─── FLIP WEAPON ────────────────────────────────────────────

    private void FlipWeaponWithPlayer()
    {
        if (weaponObject == null || playerSpriteRenderer == null) return;

        Vector3 weaponPos = weaponObject.transform.localPosition;
        Vector3 weaponScale = weaponObject.transform.localScale;

        if (isFacingRight)
        {
            weaponPos.x = -weaponPos.x;
            weaponScale.x = -Mathf.Abs(weaponScale.x);
        }
        else
        {
            weaponScale.x = Mathf.Abs(weaponScale.x);
        }

        weaponObject.transform.localPosition = weaponPos;
        weaponObject.transform.localScale = weaponScale;
    }
}