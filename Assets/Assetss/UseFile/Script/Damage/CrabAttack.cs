using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrabAttack : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 0.5f;

    private CapsuleCollider2D attackCollider;
    private bool hasHitThisActivation = false;

    void Awake()
    {
        attackCollider = GetComponent<CapsuleCollider2D>();
    }

    void OnEnable()
    {
        hasHitThisActivation = false;
    }

    void FixedUpdate()
    {
        // Every physics frame while active, check if player is overlapping
        if (hasHitThisActivation) return;
        if (attackCollider == null) return;

        Vector2 center = (Vector2)transform.position + attackCollider.offset;
        float radius = attackCollider.size.x * 0.5f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                ApplyDamageAndStun(hit);
                hasHitThisActivation = true;
                break;
            }
        }
    }

    private void ApplyDamageAndStun(Collider2D playerCollider)
    {
        var player = playerCollider.GetComponent<PlayerCharacter>();
        if (player == null) return;

        // Deal skill damage from parent Crab
        var crab = GetComponentInParent<EnemyCrab>();
        if (crab != null)
        {
            player.TakeDamage(crab.skillDamage);
        }
        else
        {
            Debug.LogWarning($"{name}: Could not find EnemyCrab component in parents.");
        }

        // Stun the player: freeze movement for stunDuration
        var playerController = playerCollider.GetComponent<PlayerCharacterController>();
        if (playerController != null)
        {
            playerController.StartCoroutine(StunPlayer(playerController, playerCollider));
        }
    }

    private IEnumerator StunPlayer(PlayerCharacterController controller, Collider2D playerCollider)
    {
        // Trigger "isStun" animator parameter
        Animator playerAnimator = playerCollider.GetComponent<Animator>();

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("isStun");
            playerAnimator.Play("getStun");
        }

        // Disable player controller to freeze movement
        controller.enabled = false;

        // Stop player velocity
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(stunDuration);

        // Re-enable player movement
        controller.enabled = true;
    }
}
