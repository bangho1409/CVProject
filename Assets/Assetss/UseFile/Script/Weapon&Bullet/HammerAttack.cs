using UnityEngine;

public class HammerAttack : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Hammer GameObject child of Player (contains SpriteRenderer + Animator).")]
    [SerializeField] private GameObject hammerObject;

    [Tooltip("Player Animator (same one used by PlayerCharacterController).")]
    [SerializeField] private Animator playerAnimator;

    [Tooltip("Player SpriteRenderer (to read flipX for facing direction).")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Weapon Switcher")]
    [Tooltip("WeaponSwitcher to temporarily disable gun/rifle during hammer attack.")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    // Data from CSV (BulletBase ID = 100 → Melee)
    private BaseBulletData hammerData;

    // Attack state
    private bool isAttacking = false;
    private bool canAttack = true;
    private float cooldownTimer = 0f;
    private float delayTimer = 0f;
    private bool waitingForDelay = false;
    private bool colliderSpawned = false;

    // Prefab loaded from Resources
    private GameObject attackColliderPrefab;

    // Spawned collider instance (so we can destroy it)
    private GameObject activeCollider;

    void Start()
    {
        // Load hammer data from CSV (ID 100 = Melee)
        if (BaseBulletDataManager.Instance != null)
        {
            hammerData = BaseBulletDataManager.Instance.GetBulletById(100);
            if (hammerData != null)
            {
                // Load the attack collider prefab from Resources
                string path = hammerData.prefabPath.Replace(".prefab", "");
                attackColliderPrefab = Resources.Load<GameObject>(path);
                if (attackColliderPrefab == null)
                {
                    Debug.LogWarning($"HammerAttack: Could not load prefab at '{path}'.");
                }
            }

        }

        // Initially disable the hammer visual
        if (hammerObject != null)
        {
            hammerObject.SetActive(false);
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

        // Handle delay: wait delayTime after pressing attack, then spawn the collider
        if (waitingForDelay)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                waitingForDelay = false;
                SpawnAttackCollider();
            }
        }

        // Flip hammer to match player facing direction
        FlipHammerWithPlayer();
    }

    /// <summary>
    /// Called by UI Button (Hammer button) or InputSystem.
    /// </summary>
    public void OnHammerAttack()
    {
        if (!canAttack || isAttacking || hammerData == null)
            return;

        isAttacking = true;
        canAttack = false;
        colliderSpawned = false;

        // Calculate total attack duration
        float baseDuration = hammerData.delayTime + 0.6f;
        float attackDuration = baseDuration / Mathf.Max(hammerData.speed, 0.1f);

        // Temporarily disable gun/rifle (no timer — HammerAttack controls re-enable)
        if (weaponSwitcher != null)
        {
            weaponSwitcher.DisableForHammer();
        }

        // Activate hammer visual
        if (hammerObject != null)
        {
            hammerObject.SetActive(true);
        }

        // Trigger player melee animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("isMelee");
        }

        // Start delay timer (DelayTime from CSV: time before the hit collider spawns)
        waitingForDelay = true;
        delayTimer = hammerData.delayTime;

        Invoke(nameof(EndAttack), attackDuration);
    }

    private void SpawnAttackCollider()
    {
        if (attackColliderPrefab == null || colliderSpawned)
            return;

        colliderSpawned = true;

        // Determine attack offset direction based on player facing
        bool facingRight = playerSpriteRenderer != null && playerSpriteRenderer.flipX;
        Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;

        // Spawn the collider in front of the player, offset by radius
        Vector3 spawnPos = transform.position + (Vector3)(attackDirection * hammerData.radius);
        activeCollider = Instantiate(attackColliderPrefab, spawnPos, Quaternion.identity);

        // Parent to this transform so it follows the player
        activeCollider.transform.SetParent(transform);

        float scaleX = facingRight ? -hammerData.radius : hammerData.radius;
        activeCollider.transform.localScale = new Vector3(scaleX, hammerData.radius, hammerData.radius);

        // Attach the damage component and set damage from CSV
        HammerDamageCollider damageComp = activeCollider.GetComponent<HammerDamageCollider>();
        if (damageComp == null)
        {
            damageComp = activeCollider.AddComponent<HammerDamageCollider>();
        }
        damageComp.damage = hammerData.damage;

        // Destroy the collider after a short time so it only hits once per swing
        float colliderLifetime = 0.3f / Mathf.Max(hammerData.speed, 0.1f);
        Destroy(activeCollider, colliderLifetime);
    }

    /// <summary>
    /// Ends the attack: disable hammer visual, start cooldown,
    /// and notify WeaponSwitcher to re-enable gun/rifle.
    /// </summary>
    private void EndAttack()
    {
        isAttacking = false;
        waitingForDelay = false;

        // Disable hammer visual
        if (hammerObject != null)
        {
            hammerObject.SetActive(false);
        }

        // Destroy lingering collider if still active
        if (activeCollider != null)
        {
            Destroy(activeCollider);
        }

        // Start cooldown (from CSV)
        cooldownTimer = hammerData != null ? hammerData.cooldown : 1f;

        // NOW re-enable gun/rifle — guaranteed after hammer is fully done
        if (weaponSwitcher != null)
        {
            weaponSwitcher.ReEnableAfterHammer();
        }
    }

    /// <summary>
    /// Flips the hammer object to follow the player's facing direction.
    /// </summary>
    private void FlipHammerWithPlayer()
    {
        if (hammerObject == null || playerSpriteRenderer == null)
            return;

        Vector3 hammerScale = hammerObject.transform.localScale;
        Vector3 hammerPos = hammerObject.transform.localPosition;

        if (playerSpriteRenderer.flipX)
        {
            // Player facing right (flipX = true)
            hammerScale.x = -Mathf.Abs(hammerScale.x);
            hammerPos.x = Mathf.Abs(hammerPos.x);
        }
        else
        {
            // Player facing left (flipX = false)
            hammerScale.x = Mathf.Abs(hammerScale.x);
            hammerPos.x = -Mathf.Abs(hammerPos.x);
        }

        hammerObject.transform.localScale = hammerScale;
        hammerObject.transform.localPosition = hammerPos;
    }

    /// <summary>
    /// Returns the direction the player is currently facing.
    /// </summary>
    private Vector2 GetFacingDirection()
    {
        if (playerSpriteRenderer != null && playerSpriteRenderer.flipX)
        {
            return Vector2.right;
        }
        return Vector2.left;
    }
}