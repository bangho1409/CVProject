using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public static StatusBar Instance;
    private CharacterData playerCharacterData;

    // assignable in inspector as fallback
    public GameObject hpBarFill;
    public GameObject staminaBarFill;

    private RectTransform hpRect;
    private RectTransform staminaRect;
    private float hpMaxWidth;
    private float staminaMaxWidth;

    void Awake()
    {
        Instance = this;
        AssignBarReferences();

        hpRect = hpBarFill != null ? hpBarFill.GetComponent<RectTransform>() : null;
        staminaRect = staminaBarFill != null ? staminaBarFill.GetComponent<RectTransform>() : null;

        if (hpRect != null)
            hpMaxWidth = hpRect.rect.width;
        if (staminaRect != null)
            staminaMaxWidth = staminaRect.rect.width;

        // safety log
        if (hpBarFill == null) Debug.LogWarning($"{name}: hpBarFill not found. Assign it in inspector or check hierarchy/name.");
        if (staminaBarFill == null) Debug.LogWarning($"{name}: staminaBarFill not found. Assign it in inspector or check hierarchy/name.");
    }

    void Start()
    {
        UpdateBarsImmediate();
    }

    void Update()
    {
        UpdateBarsImmediate();
    }

    private void AssignBarReferences()
    {
        // If user has already assigned them in inspector, respect that.
        if (hpBarFill != null && staminaBarFill != null)
            return;

        // Try common direct child names first (fast)
        hpBarFill ??= transform.Find("hpBarFill")?.gameObject;
        staminaBarFill ??= transform.Find("staminaBarFill")?.gameObject;

        // Try likely nested paths (adjust if your hierarchy is different)
        hpBarFill ??= transform.Find("HP/hpBarFill")?.gameObject;
        staminaBarFill ??= transform.Find("Stamina/staminaBarFill")?.gameObject;

        // As a robust fallback, search children recursively by name
        if (hpBarFill == null)
        {
            var t = GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "hpBarFill");
            hpBarFill = t?.gameObject;
        }
        if (staminaBarFill == null)
        {
            var t = GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "staminaBarFill");
            staminaBarFill = t?.gameObject;
        }
    }

    private void UpdateBarsImmediate()
    {
        if (PlayerCharacter.Instance == null)
            return;

        var stats = PlayerCharacter.Instance.GetPlayerCurrentStats();
        if (stats == null)
            return;

        // Use max values for denominator (avoid current/current which yields 1.0)
        float hpRatio = (stats.maxHp > 0f) ? Mathf.Clamp01(PlayerCharacter.Instance.hp / stats.maxHp) : 0f;
        float staminaRatio = (stats.maxStamina > 0f) ? Mathf.Clamp01(PlayerCharacter.Instance.stamina / stats.maxStamina) : 0f;

        if (hpRect != null)
            hpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hpMaxWidth * hpRatio);

        if (staminaRect != null)
            staminaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, staminaMaxWidth * staminaRatio);
    }
}
