using UnityEngine;

public class EnemyBat : MonoBehaviour
{
    private enum BatState
    {
        WaitCooldown,
        FlyToPlayer,
        Pause,
        Dash,
        Retreat
    }

    private EnemyData enemyData;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;
    private GameObject playerGameObject;

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

    [Header("Attack Tuning")]
    [SerializeField] private float pauseBeforeAttack = 0.6f;
    [SerializeField] private float dashDistanceMultiplier = 3f;

    [Header("Idle Hover")]
    [SerializeField] private float hoverAmplitude = 0.3f;
    [SerializeField] private float hoverFrequency = 2f;

    [Header("Touch Damage")]
    [SerializeField] private float touchDamageInterval = 0.5f;

    private BatState state = BatState.FlyToPlayer;
    private bool playerInAttackZone = false;
    private float pauseEndTime;
    private float nextTouchDamageTime = -Mathf.Infinity;

    private Vector2 dashDirection = Vector2.zero;
    private Vector2 dashStartPos;
    private float dashTotalDistance;
    private bool dashDamageDealt = false;
    public bool IsDashing { get; private set; } = false;

    private Vector2 retreatDirection;
    private Vector2 retreatStartPos;

    private float skillCooldownEndTime = -Mathf.Infinity;
    private float hoverTimer;

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
            enemyData = EnemiesDataManager.Instance.GetCharacterById(201);
            if (enemyData != null)
                LoadStats();
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;
        if (hp <= 0) return;

        RecoverStamina();
        RotateTowardsPlayer();

        switch (state)
        {
            case BatState.WaitCooldown: DoWaitCooldown(); break;
            case BatState.FlyToPlayer:  FlyToPlayer();    break;
            case BatState.Pause:        DoPause();        break;
            case BatState.Dash:         DoDash();         break;
            case BatState.Retreat:      DoRetreat();      break;
        }
    }

    private void DoWaitCooldown()
    {
        hoverTimer += Time.fixedDeltaTime;

        float speed = moveSpeed * 0.5f;
        float angle = hoverTimer * hoverFrequency * Mathf.PI * 2f;
        float vx = Mathf.Cos(angle) * hoverAmplitude * speed;
        float vy = Mathf.Sin(angle) * hoverAmplitude * speed;
        rb.linearVelocity = new Vector2(vx, vy);

        if (Time.time >= skillCooldownEndTime)
        {
            state = BatState.FlyToPlayer;
            hoverTimer = 0f;
        }
    }

    private void FlyToPlayer()
    {
        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        HandleTouchDamage();

        if (playerInAttackZone && CanAttack())
            EnterPause();
    }

    private void EnterPause()
    {
        state = BatState.Pause;
        rb.linearVelocity = Vector2.zero;
        pauseEndTime = Time.time + pauseBeforeAttack;

        if (animator != null)
            animator.SetTrigger("attackRange");
    }

    private void DoPause()
    {
        rb.linearVelocity = Vector2.zero;

        if (Time.time >= pauseEndTime)
            StartDash();
    }

    private void StartDash()
    {
        if (playerTransform == null) return;

        stamina -= skillStaminaCost;

        dashDirection = ((Vector2)playerTransform.position - rb.position).normalized;
        if (dashDirection == Vector2.zero) dashDirection = transform.right;

        dashStartPos = rb.position;
        float distToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        dashTotalDistance = distToPlayer + dashDistanceMultiplier;
        dashDamageDealt = false;

        IsDashing = true;
        state = BatState.Dash;

        if (animator != null)
            animator.Play("bat_attack");
    }

    private void DoDash()
    {
        rb.linearVelocity = dashDirection * dashSpeed;

        if (!dashDamageDealt)
        {
            float distToPlayer = Vector2.Distance(rb.position, playerTransform.position);
            if (distToPlayer <= 0.8f)
            {
                var player = playerGameObject.GetComponent<PlayerCharacter>();
                if (player != null)
                {
                    player.TakeDamage(skillDamage);
                    dashDamageDealt = true;
                }
            }
        }

        float traveled = Vector2.Distance(dashStartPos, rb.position);
        if (traveled >= dashTotalDistance)
            EndDash();
    }

    private void EndDash()
    {
        IsDashing = false;
        rb.linearVelocity = Vector2.zero;
        EnterRetreat();
    }

    private void EnterRetreat()
    {
        state = BatState.Retreat;

        retreatDirection = (rb.position - (Vector2)playerTransform.position).normalized;
        if (retreatDirection == Vector2.zero) retreatDirection = -transform.right;

        retreatStartPos = rb.position;
    }

    private void DoRetreat()
    {
        rb.linearVelocity = retreatDirection * moveSpeed;

        float traveled = Vector2.Distance(retreatStartPos, rb.position);
        if (traveled >= retreatDistance)
        {
            rb.linearVelocity = Vector2.zero;
            skillCooldownEndTime = Time.time + skillCooldown;
            hoverTimer = 0f;
            state = BatState.WaitCooldown;
        }
    }

    private void HandleTouchDamage()
    {
        if (playerGameObject == null) return;
        if (state == BatState.Dash || state == BatState.Retreat || state == BatState.WaitCooldown) return;

        float dist = Vector2.Distance(rb.position, playerTransform.position);
        if (dist <= 0.5f && Time.time >= nextTouchDamageTime)
        {
            var player = playerGameObject.GetComponent<PlayerCharacter>();
            if (player != null)
            {
                player.TakeDamage(touchAttackDamage);
                nextTouchDamageTime = Time.time + touchDamageInterval;
                EnterRetreat();
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector2 dir;
        if (state == BatState.Retreat)
            dir = retreatDirection;
        else if (state == BatState.Dash)
            dir = dashDirection;
        else
            dir = ((Vector2)playerTransform.position - rb.position).normalized;

        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.localScale = dir.x < 0 ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool CanAttack()
    {
        return Time.time >= skillCooldownEndTime && stamina >= skillStaminaCost;
    }

    private void RecoverStamina()
    {
        if (enemyData != null && stamina < enemyData.stamina)
        {
            stamina += recoveryStaminaRate * Time.fixedDeltaTime;
            if (stamina > enemyData.stamina) stamina = enemyData.stamina;
        }
    }

    private void LoadStats()
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

    public void OnPlayerEnterAttackZone()
    {
        playerInAttackZone = true;

        if (state == BatState.FlyToPlayer && CanAttack())
            EnterPause();
    }

    public void OnPlayerExitAttackZone()
    {
        playerInAttackZone = false;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        animator.SetTrigger("getHit");

        if (hp <= 0)
            Die();
    }

    private void Die()
    {
        if (animator != null) animator.SetTrigger("death");
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

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
