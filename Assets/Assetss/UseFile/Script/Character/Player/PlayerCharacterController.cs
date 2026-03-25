using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    private PlayerCharacter playerCharacter;

    [SerializeField] private Animator animator;

    private bool runButtonPressed;
    private bool dashButtonTriggered;

    private bool isDashing = false;
    private float dashTime;
    private Vector2 dashDirection;

    [SerializeField] private float dashRecoveryDuration = 0.35f;
    private Vector2 currentVelocity = Vector2.zero;

    private bool isDecelerating = false;
    private float decelTimer;
    private Vector2 decelDirection;
    private float decelStartSpeed;

    private ContactFilter2D dashContactFilter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCharacter = GetComponent<PlayerCharacter>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        dashContactFilter = new ContactFilter2D();
        dashContactFilter.useTriggers = false;
        dashContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        dashContactFilter.useLayerMask = true;
    }

    void Update()
    {
        if (playerCharacter == null)
        {
            return;
        }

        // Facing
        if (!isDashing)
        {
            if (movement.x < 0)
            {
                spriteRenderer.flipX = false; // Face left
            }
            else if (movement.x > 0)
            {
                spriteRenderer.flipX = true; // Face right
            }

            if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || dashButtonTriggered)
            {
                if (movement != Vector2.zero && playerCharacter.stamina >= playerCharacter.dashStaminaCost)
                {
                    StartDash(movement);
                }
                dashButtonTriggered = false;
            }
        }

        if (animator != null)
        {
            animator.SetBool("isDashing", isDashing);

            // Check if player is holding a weapon (Gun or Rifle)
            bool isHoldingWeapon = animator.GetBool("isHolding");

            bool animRunning = !isDashing && runButtonPressed && movement != Vector2.zero && playerCharacter.stamina > playerCharacter.runStaminaCostPerSecond;
            // When holding gun/rifle and running, keep gun_walk_side/rifle_walk_side animation
            // but still flag isRunning=false so the player doesn't switch to run animation
            if (isHoldingWeapon)
            {
                animator.SetBool("isRunning", false);
            }
            else
            {
                animator.SetBool("isRunning", animRunning);
            }

            bool animMoving = !isDashing && movement != Vector2.zero;
            animator.SetBool("isMoving", animMoving);
        }
    }

    void FixedUpdate()
    {
        if (playerCharacter == null)
        {
            return;
        }

        if (isDashing)
        {
            float dashStep = playerCharacter.dashSpeed * Time.fixedDeltaTime;
            RaycastHit2D[] results = new RaycastHit2D[1];

            // Use configured filter so it respects layers and ignores triggers
            int hitCount = rb.Cast(dashDirection, dashContactFilter, results, dashStep);

            if (hitCount > 0 && results[0].collider != null)
            {
                // Stop just before the wall (subtract small skin width to avoid overlap)
                float stopDistance = Mathf.Max(results[0].distance - 0.01f, 0f);
                rb.MovePosition(rb.position + dashDirection * stopDistance);
                EndDash();
            }
            else
            {
                rb.MovePosition(rb.position + dashDirection * dashStep);
                dashTime -= Time.fixedDeltaTime;
                if (dashTime <= 0)
                {
                    EndDash();
                }
            }

            return;
        }

        // Determine current base speed (running or walking)
        bool isRunning = runButtonPressed && playerCharacter.stamina > playerCharacter.runStaminaCostPerSecond;
        float baseSpeed = isRunning ? playerCharacter.runSpeed : playerCharacter.moveSpeed;

        if (isDecelerating)
        {
            decelTimer -= Time.fixedDeltaTime;
            float t = Mathf.Clamp01(decelTimer / dashRecoveryDuration);
            float decelSpeed = Mathf.Lerp(baseSpeed, decelStartSpeed, t);
            Vector2 decelVel = decelDirection * decelSpeed;

            Vector2 inputVel = movement * baseSpeed;

            currentVelocity = Vector2.Lerp(inputVel, decelVel, t);

            if (decelTimer <= 0f)
            {
                isDecelerating = false;
            }
        }
        else
        {
            currentVelocity = movement * baseSpeed;
        }

        // Use MovePosition instead of setting velocity directly.
        // This lets the physics engine handle collision resolution properly
        // and prevents the player from being "pushed" through walls.
        Vector2 newPos = rb.position + currentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Consume running stamina when moving and running
        if (isRunning && movement != Vector2.zero)
        {
            playerCharacter.ConsumeStamina(playerCharacter.runStaminaCostPerSecond * Time.fixedDeltaTime);
        }
    }

    // Centralized dash starter
    private void StartDash(Vector2 dir)
    {
        if (playerCharacter == null)
        {
            return;
        }

        isDashing = true;
        dashTime = playerCharacter.dashDuration;
        dashDirection = dir.normalized;

        playerCharacter.ConsumeStamina(playerCharacter.dashStaminaCost);

        // Invincible during dash
        playerCharacter.SetInvincible(true);

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isMoving", false);
            animator.SetBool("isDashing", true);
        }
    }

    private void EndDash()
    {
        isDashing = false;

        // Vulnerable again after dash
        if (playerCharacter != null)
        {
            playerCharacter.SetInvincible(false);
        }

        // Reset smoothed velocity to prevent leftover momentum from pushing through walls
        currentVelocity = Vector2.zero;

        isDecelerating = true;
        decelTimer = dashRecoveryDuration;
        decelDirection = dashDirection;
        decelStartSpeed = playerCharacter != null ? playerCharacter.dashSpeed : 0f;

        if (animator != null)
        {
            animator.SetBool("isDashing", false);
            animator.SetBool("isMoving", movement != Vector2.zero);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        movement = input.sqrMagnitude > 1f ? input.normalized : input;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        runButtonPressed = context.ReadValueAsButton();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashButtonTriggered = true;
        }
    }

}