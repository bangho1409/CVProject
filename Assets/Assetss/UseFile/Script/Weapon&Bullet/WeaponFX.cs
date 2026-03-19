using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("WeaponFX: No Animator found on this GameObject.");
        }
    }

    void Update()
    {
        if (animator == null) return;

        // Get the current state info from base layer (0)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // When the "explosion" animation has finished playing (normalizedTime >= 1),
        // the state transitions to Exit → destroy this GameObject
        if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0))
        {
            Destroy(gameObject);
        }
    }
}
