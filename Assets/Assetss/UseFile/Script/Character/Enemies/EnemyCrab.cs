using UnityEngine;
using System.Collections;

public class EnemyCrab : MonoBehaviour
{
    private enum CrabState
    {
        MoveToPlayer,
        Pause,
        Attack,
        WaitCooldown
    }

    // Data + components
    private EnemyData enemyData;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;
    private GameObject playerGameObject;
    public GameObject AttackHitbox;

    // Stats (populated from CSV)
    public int id;
    public string characterName;
    public int type;
    public float hp;
    public float stamina;
    public float skillStaminaCost;
    public float touchAttackDamage;
    public float moveSpeed;
    public float runSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float recoveryStaminaRate;
    public float skillDamage;
    public float skillCooldown;
    public float retreatDistance;

    [Header("Touch Damage")]
    [SerializeField] private float touchDamageInterval = 0.5f;
    private float nextTouchDamageTime = -Mathf.Infinity;

    [Header("Pause Before Attack")]
    [SerializeField] private float pauseBeforeAttack = 0.3f;
    private float pauseEndTime;

    // Wall avoidance (private, not serialized)
    private const float WALL_CHECK_DISTANCE = 0.6f;
    private ContactFilter2D wallFilter;

    private CrabState state = CrabState.MoveToPlayer;
    private bool playerInAttackZone = false;
    private float skillCooldownEndTime = -Mathf.Infinity;

    public bool isDead { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerGameObject = player;
        }

        if (EnemiesDataManager.Instance != null)
        {
            enemyData = EnemiesDataManager.Instance.GetCharacterById(200);
            if (enemyData != null)
                LoadStats();
        }

        // Ensure attack hitbox starts disabled
        if (AttackHitbox != null)
            AttackHitbox.SetActive(false);

        // Configure wall filter: solid colliders only, respect collision matrix
        wallFilter = new ContactFilter2D();
        wallFilter.useTriggers = false;
        wallFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        wallFilter.useLayerMask = true;
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;
        if (isDead || hp <= 0) return;

        RecoverStamina();
        HandleTouchDamage();
        RotateTowardsPlayer();

        switch (state)
        {
            case CrabState.MoveToPlayer:  MoveToPlayer();    break;
            case CrabState.Pause:         DoPause();         break;
            case CrabState.Attack:        DoAttack();        break;
            case CrabState.WaitCooldown:  DoWaitCooldown();  break;
        }
    }

    // --- States ---

    private void MoveToPlayer()
    {
        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 desiredVelocity = dir * moveSpeed;

        // Wall avoidance: cast in desired direction, if blocked → slide along wall
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = rb.Cast(dir, wallFilter, hits, WALL_CHECK_DISTANCE);

        if (hitCount > 0 && hits[0].collider != null)
        {
            // Slide along the wall by removing the component going into the wall
            Vector2 wallNormal = hits[0].normal;
            desiredVelocity = desiredVelocity - Vector2.Dot(desiredVelocity, wallNormal) * wallNormal;

            // If almost completely blocked (corner), try perpendicular directions
            if (desiredVelocity.magnitude < moveSpeed * 0.1f)
            {
                Vector2 perp1 = new Vector2(-dir.y, dir.x);
                Vector2 perp2 = new Vector2(dir.y, -dir.x);

                // Pick the perpendicular direction closer to the player
                Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position);
                desiredVelocity = Vector2.Dot(toPlayer, perp1) > 0
                    ? perp1 * moveSpeed * 0.5f
                    : perp2 * moveSpeed * 0.5f;
            }
        }

        // Use MovePosition so physics engine handles collision properly
        Vector2 newPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        animator.SetBool("isMoving", true);

        // If CrabAttackCensor detects Player and has enough stamina -> pause before attack
        if (playerInAttackZone && stamina >= skillStaminaCost)
        {
            stamina -= skillStaminaCost;
            pauseEndTime = Time.time + pauseBeforeAttack;
            state = CrabState.Pause;
        }
    }

    private void DoPause()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("isAttack");

        if (Time.time >= pauseEndTime)
        {
            state = CrabState.Attack;
        }
    }

    private void DoAttack()
    {
        rb.linearVelocity = Vector2.zero;
        if (AttackHitbox != null)
        {
            AttackHitbox.SetActive(true);
        }

        // Transition to cooldown immediately, but hitbox stays until animation ends
        state = CrabState.WaitCooldown;
        skillCooldownEndTime = Time.time + skillCooldown;

        StartCoroutine(DisableHitboxAfterAnimation());
    }

    private IEnumerator DisableHitboxAfterAnimation()
    {
        // Wait one frame so animator updates to the "Attack" state
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Wait until the "Attack" animation finishes playing
        while (stateInfo.IsName("Attack") && stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        if (AttackHitbox != null)
            AttackHitbox.SetActive(false);
    }

    private void DoWaitCooldown()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        if (Time.time >= skillCooldownEndTime)
        {
            state = CrabState.MoveToPlayer;
        }
    }

    // --- Touch Damage (tick-based, like Bat) ---

    private void HandleTouchDamage()
    {
        if (playerGameObject == null) return;

        float dist = Vector2.Distance(rb.position, playerTransform.position);
        if (dist <= 0.5f && Time.time >= nextTouchDamageTime)
        {
            var player = playerGameObject.GetComponent<PlayerCharacter>();
            if (player != null)
            {
                player.TakeDamage(touchAttackDamage);
                nextTouchDamageTime = Time.time + touchDamageInterval;
            }
        }
    }

    // --- Helpers ---

    void LoadStats()
    {
        id = enemyData.id;
        characterName = enemyData.name;
        type = enemyData.type;
        hp = enemyData.hp;
        stamina = enemyData.stamina;
        skillStaminaCost = enemyData.skillStaminaCost;
        touchAttackDamage = enemyData.touchAttackDamage;
        moveSpeed = enemyData.moveSpeed;
        runSpeed = enemyData.runSpeed;
        dashSpeed = enemyData.dashSpeed;
        dashDuration = enemyData.dashDuration;
        recoveryStaminaRate = enemyData.recoveryStaminaRate;
        skillDamage = enemyData.skillDamage;
        skillCooldown = enemyData.skillCooldown;
        retreatDistance = enemyData.retreatDistance;
    }

    void RecoverStamina()
    {
        if (enemyData != null && stamina < enemyData.stamina)
        {
            stamina += recoveryStaminaRate * Time.fixedDeltaTime;
            if (stamina > enemyData.stamina) stamina = enemyData.stamina;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.localScale = dir.x < 0 ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // --- Public API (called by CrabAttackZoneCensor) ---

    public void OnPlayerEnterAttackZone()
    {
        playerInAttackZone = true;
    }

    public void OnPlayerExitAttackZone()
    {
        playerInAttackZone = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        hp -= damage;
        if (animator != null) animator.SetTrigger("getHit");

        if (hp <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Disable ALL colliders (root + children like CrabAttackCensor, aimPoint)
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // Change tag so auto-aim no longer targets this enemy
        gameObject.tag = "Untagged";
        foreach (Transform child in transform)
        {
            child.gameObject.tag = "Untagged";
        }

        // Change layer so OverlapCircle/layer checks skip this enemy
        int defaultLayer = LayerMask.NameToLayer("Default");
        gameObject.layer = defaultLayer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = defaultLayer;
        }

        if (animator != null) animator.SetBool("isDeath", true);

        // Spawn drop item
        if (enemyData != null && !string.IsNullOrEmpty(enemyData.itemDropPath))
        {
            GameObject dropPrefab = Resources.Load<GameObject>(enemyData.itemDropPath);
            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 1f);
    }
}
