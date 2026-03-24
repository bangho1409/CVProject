using UnityEngine;

public class GunAttack : MonoBehaviour
{
    public enum WeaponType { Gun, Rifle }

    [Header("Weapon Type")]
    [SerializeField] private WeaponType weaponType = WeaponType.Gun;

    [Header("References")]
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Auto-Aim Settings")]
    [SerializeField] private float autoAimRange = 7f;

    private BaseBulletData bulletData;
    private GameObject bulletPrefab;

    private bool isActive = false;
    private float cooldownTimer = 0f;
    private bool canShoot = true;
    private float delayTimer = 0f;
    private bool waitingForDelay = false;

    private bool disabledByHammer = false;

    private float muzzleFlashTimer = 0f;
    private const float MUZZLE_FLASH_DURATION = 0.08f;

    private Transform nearestEnemy;
    private bool isFacingRight = false;
    private bool isMaxBulletLevel = false;

    [Header("Bullet Data")]
    [SerializeField] private int bulletDataId = 200;

    void Awake()
    {
        if (BaseBulletDataManager.Instance != null)
        {
            bulletData = BaseBulletDataManager.Instance.GetBulletById(bulletDataId);
            if (bulletData != null)
            {
                string path = bulletData.prefabPath.Replace(".prefab", "");
                bulletPrefab = Resources.Load<GameObject>(path);

                // Check if this is already the max bullet level
                isMaxBulletLevel = BaseBulletDataManager.Instance.GetBulletById(bulletDataId + 1) == null;
            }
        }

        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }
    }

    void Start()
    {
        if (BaseBulletDataManager.Instance != null)
        {
            bulletData = BaseBulletDataManager.Instance.GetBulletById(bulletDataId);
            if (bulletData != null)
            {
                string path = bulletData.prefabPath.Replace(".prefab", "");
                bulletPrefab = Resources.Load<GameObject>(path);

                // Check if this is already the max bullet level
                isMaxBulletLevel = BaseBulletDataManager.Instance.GetBulletById(bulletDataId + 1) == null;
            }
        }
    }

    void Update()
    {
        if (!isActive || disabledByHammer) return;

        if (!canShoot)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canShoot = true;
            }
        }

        if (waitingForDelay)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                waitingForDelay = false;
                FireBullet();
                cooldownTimer = bulletData.cooldown;
            }
        }

        FindNearestEnemy();

        if (nearestEnemy != null && canShoot && !waitingForDelay)
        {
            StartShot();
        }

        AutoFaceNearestEnemy();
        isFacingRight = playerSpriteRenderer != null && playerSpriteRenderer.flipX;

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
    /// </summary>
    public void LevelUpBullet()
    {
        if (isMaxBulletLevel || BaseBulletDataManager.Instance == null || bulletData == null) return;

        BaseBulletData nextData = BaseBulletDataManager.Instance.GetBulletById(bulletData.id + 1);
        if (nextData != null)
        {
            bulletData = nextData;
            bulletDataId = nextData.id;

            string path = bulletData.prefabPath.Replace(".prefab", "");
            GameObject newPrefab = Resources.Load<GameObject>(path);
            if (newPrefab != null)
            {
                bulletPrefab = newPrefab;
            }

            // Check if the NEW level is the max
            isMaxBulletLevel = BaseBulletDataManager.Instance.GetBulletById(bulletData.id + 1) == null;
        }
        else
        {
            isMaxBulletLevel = true;
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public GameObject GetWeaponObject()
    {
        return weaponObject;
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
            playerSpriteRenderer.flipX = true;
        else if (dirX < -0.05f)
            playerSpriteRenderer.flipX = false;
    }

    private Vector2 GetShootDirection()
    {
        if (nearestEnemy != null)
            return (nearestEnemy.position - shootingPoint.position).normalized;

        return GetFacingDirection();
    }

    private Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
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