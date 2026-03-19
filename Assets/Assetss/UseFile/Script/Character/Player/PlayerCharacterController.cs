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

    // Smoothing & post-dash deceleration
    [SerializeField] private float movementSmoothing = 0.08f;        // smaller = snappier
    [SerializeField] private float dashRecoveryDuration = 0.35f;    // time to fade from dash speed -> normal
    private Vector2 velocitySmoothRef = Vector2.zero;

    private bool isDecelerating = false;
    private float decelTimer;
    private Vector2 decelDirection;
    private float decelStartSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCharacter = GetComponent<PlayerCharacter>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
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
            // Dash movement with collision check (unchanged)
            float dashStep = playerCharacter.dashSpeed * Time.fixedDeltaTime;
            RaycastHit2D[] results = new RaycastHit2D[1];
            int hitCount = rb.Cast(dashDirection, new ContactFilter2D(), results, dashStep);

            if (hitCount > 0 && results[0].collider != null)
            {
                float stopDistance = results[0].distance;
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

        // Handle deceleration phase after dash
        Vector2 desiredVelocity;
        if (isDecelerating)
        {
            decelTimer -= Time.fixedDeltaTime;
            float t = Mathf.Clamp01(decelTimer / dashRecoveryDuration);
            float decelSpeed = Mathf.Lerp(baseSpeed, decelStartSpeed, t);
            Vector2 decelVel = decelDirection * decelSpeed;

            Vector2 inputVel = movement * baseSpeed;

            desiredVelocity = Vector2.Lerp(inputVel, decelVel, t);

            if (decelTimer <= 0f)
            {
                isDecelerating = false;
            }
        }
        else
        {
            desiredVelocity = movement * baseSpeed;
        }

        // Smooth movement
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, desiredVelocity, ref velocitySmoothRef, movementSmoothing);
        }
        else
        {
            Vector2 targetPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
            Vector2 lerped = Vector2.Lerp(rb.position, targetPos, Mathf.Clamp01(1f - Mathf.Exp(-movementSmoothing * 60f * Time.fixedDeltaTime)));
            rb.MovePosition(lerped);
        }

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