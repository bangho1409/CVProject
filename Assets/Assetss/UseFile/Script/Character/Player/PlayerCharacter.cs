using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static PlayerCharacter Instance;
    private CharacterData playerCharacterData;

    // Các thuộc tính của nhân vật sẽ được gán từ playerCharacterData
    public int id;
    public string characterName;
    public int type;
    public float hp;
    public float moveSpeed;
    public float runSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float stamina;
    public float runStaminaCostPerSecond;
    public float dashStaminaCost;
    public float StaminaRecoveryRate;
    public float experiencePoints;

    // Maximum values read from data (used as clamp / for UI scaling)
    [HideInInspector]
    public float maxHp { get; private set; }
    public float maxStamina { get; private set; }
    public float expGainPoint { get; private set; }

    public bool isDead { get; private set; }

    // Invincible during dash
    public bool isInvincible { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (CharacterDataManager.Instance != null)
        {
            playerCharacterData = CharacterDataManager.Instance.GetCharacterById(100); //Player ID trong CSV là 100
            if (playerCharacterData != null)
            {
                GetStat();
                expGainPoint = 0f;
            }
            else
            {
                Debug.LogWarning($"{name}: No character data found for id 100.");
            }
        }
        else
        {
            Debug.LogWarning($"{name}: CharacterDataManager.Instance is null. Is it in the scene?");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (playerCharacterData != null)
        {
            RecoverStaminaOverTime();
        }
    }
    //Get Stats from Data
    void GetStat()
    {
        id = playerCharacterData.id;
        characterName = playerCharacterData.name;
        type = playerCharacterData.type;
        hp = playerCharacterData.hp;
        moveSpeed = playerCharacterData.moveSpeed;
        runSpeed = playerCharacterData.runSpeed;
        dashSpeed = playerCharacterData.dashSpeed;
        dashDuration = playerCharacterData.dashDuration;
        stamina = playerCharacterData.stamina;
        maxHp = playerCharacterData.hp;
        maxStamina = playerCharacterData.stamina;
        runStaminaCostPerSecond = playerCharacterData.runStaminaCost;
        dashStaminaCost = playerCharacterData.dashStaminaCost;
        StaminaRecoveryRate = playerCharacterData.recoveryStaminaRate;
        experiencePoints = playerCharacterData.experiencePoints;
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public bool TakeDamage(float damage)
    {
        if (isDead) return true;
        if (isInvincible) return false;

        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
            return true;
        }
        return false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // Disable movement controller
        var controller = GetComponent<PlayerCharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Stop any remaining velocity
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Play Dead animation
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isDead", true);
        }
    }

    public bool Heal(float amount)
    {
        hp += amount;
        if (playerCharacterData != null && hp >= playerCharacterData.hp)
        {
            hp = playerCharacterData.hp;
            return true;
        }
        return false;
    }

    public bool ConsumeStamina(float amount)
    {
        stamina -= amount;
        if (stamina <= 0f)
        {
            stamina = 0f;
            return true;
        }
        return false;
    }

    public bool RecoverStamina(float amount)
    {
        stamina += amount;
        if (stamina >= maxStamina)
        {
            stamina = maxStamina;
            return true;
        }
        return false;
    }

    public bool GainExperience(float amount)
    {
        experiencePoints += amount;
        CharacterData nextLevelData = CharacterDataManager.Instance.GetCharacterById(id + 1);
        if (nextLevelData != null)
        {
            if (experiencePoints >= nextLevelData.experiencePoints)
            {
                playerCharacterData = nextLevelData;
                GetStat();
                return true;
            }
            else
            {
                return false;

            }
        }
        else
        {
            Debug.LogWarning($"{name}: No next level character data found for id {id + 1}.");
            return false;
        }
    }

    void RecoverStaminaOverTime()
    {
        stamina += StaminaRecoveryRate * Time.deltaTime;
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
    }

    //current player stats
    public PlayerStats GetPlayerCurrentStats()
    {
        PlayerStats stats = new PlayerStats();
        stats.id = id;
        stats.name = characterName;
        stats.type = type;
        stats.hp = hp;
        stats.stamina = stamina;
        stats.maxHp = maxHp;
        stats.maxStamina = maxStamina;
        stats.moveSpeed = moveSpeed;
        stats.runSpeed = runSpeed;
        stats.dashSpeed = dashSpeed;
        stats.dashDuration = dashDuration;
        stats.runStaminaCost = runStaminaCostPerSecond;
        stats.dashStaminaCost = dashStaminaCost;
        stats.recoveryStaminaRate = StaminaRecoveryRate;
        stats.experiencePoints = experiencePoints;
        return stats;
    }
}

[System.Serializable]
public class PlayerStats
{
    public int id;
    public string name;
    public int type;
    public float hp;           // current HP
    public float stamina;      // current stamina
    public float maxHp;        // maximum HP (from data)
    public float maxStamina;   // maximum stamina (from data)
    public float moveSpeed;
    public float runSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float runStaminaCost;
    public float dashStaminaCost;
    public float recoveryStaminaRate;
    public float experiencePoints;
}
