using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public static StatusBar Instance;

    // Assign in Inspector
    public GameObject hpBarFill;
    public GameObject staminaBarFill;
    public GameObject expBarFill;

    public Text hpText;
    public Text staminaText;
    public Text levelText;

    private RectTransform hpRect;
    private RectTransform staminaRect;
    private RectTransform expRect;
    private float hpMaxWidth;
    private float staminaMaxWidth;
    private float expMaxWidth;

    void Awake()
    {
        Instance = this;

        hpRect = hpBarFill != null ? hpBarFill.GetComponent<RectTransform>() : null;
        staminaRect = staminaBarFill != null ? staminaBarFill.GetComponent<RectTransform>() : null;
        expRect = expBarFill != null ? expBarFill.GetComponent<RectTransform>() : null;

        if (hpRect != null)
            hpMaxWidth = hpRect.rect.width;
        if (staminaRect != null)
            staminaMaxWidth = staminaRect.rect.width;
        if (expRect != null)
            expMaxWidth = expRect.rect.width;
    }

    void Start()
    {
        UpdateBarsImmediate();
    }

    void Update()
    {
        UpdateBarsImmediate();
    }

    private void UpdateBarsImmediate()
    {
        if (PlayerCharacter.Instance == null)
            return;

        var player = PlayerCharacter.Instance;
        var stats = player.GetPlayerCurrentStats();
        if (stats == null)
            return;

        // --- Level Text ---
        if (levelText != null)
            levelText.text = $"{player.characterName}";

        // --- HP Bar ---
        float hpRatio = (stats.maxHp > 0f) ? Mathf.Clamp01(player.hp / stats.maxHp) : 0f;
        if (hpRect != null)
            hpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hpMaxWidth * hpRatio);
        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(player.hp)}/{Mathf.CeilToInt(stats.maxHp)}";

        // --- Stamina Bar ---
        float staminaRatio = (stats.maxStamina > 0f) ? Mathf.Clamp01(player.stamina / stats.maxStamina) : 0f;
        if (staminaRect != null)
            staminaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, staminaMaxWidth * staminaRatio);
        if (staminaText != null)
            staminaText.text = $"{Mathf.CeilToInt(player.stamina)}/{Mathf.CeilToInt(stats.maxStamina)}";

        // --- EXP Bar ---
        if (expRect != null)
        {
            float expRatio = GetExpRatio(player);
            expRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, expMaxWidth * expRatio);
        }
    }

    private float GetExpRatio(PlayerCharacter player)
    {
        CharacterData nextLevel = CharacterDataManager.Instance != null
            ? CharacterDataManager.Instance.GetCharacterById(player.id + 1)
            : null;

        if (nextLevel == null)
            return 1f;

        float expNeeded = nextLevel.experiencePoints;
        if (expNeeded <= 0f)
            return 1f;

        return Mathf.Clamp01(player.experiencePoints / expNeeded);
    }
}
