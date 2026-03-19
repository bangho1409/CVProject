using System.Collections.Generic;
using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [Tooltip("Apply an immediate damage when an eligible character enters the trigger.")]
    public bool instantDamageOnEnter = true;

    [Tooltip("Damage applied immediately when entering (if enabled).")]
    public float instantDamageAmount = 1f;

    [Tooltip("Enable periodic damage while inside the trigger.")]
    public bool periodicDamage = false;

    [Tooltip("Damage applied each tick while the player remains inside the trigger.")]
    public float damagePerTick = 1f;

    [Tooltip("Seconds between damage ticks while player remains inside the trigger.")]
    public float tickInterval = 0.5f;

    [Tooltip("If true and periodicDamage is enabled, also apply a tick immediately on enter.")]
    public bool tickOnEnter = false;

    // Tracks players currently inside the trigger and the next allowed tick time
    private readonly Dictionary<PlayerCharacter, float> _playersNextTick = new Dictionary<PlayerCharacter, float>();

    void Start()
    {
        // Ensure the collider is a trigger (helpful warning for inspectors)
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{name}: EnemyTouchDamage expects a trigger collider (isTrigger = true).");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        var player = collision.GetComponent<PlayerCharacter>();
        if (player == null)
            return;

        if (instantDamageOnEnter)
        {
            player.TakeDamage(instantDamageAmount);
        }

        if (periodicDamage)
        {
            // schedule next tick: immediate if tickOnEnter else after tickInterval
            var next = tickOnEnter ? Time.time + Mathf.Max(0.01f, tickInterval) : Time.time + Mathf.Max(0.01f, tickInterval);
            // if tickOnEnter is true we still set next to now+interval to avoid double immediate hits on Update
            _playersNextTick[player] = next;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        var player = collision.GetComponent<PlayerCharacter>();
        if (player == null)
            return;

        if (_playersNextTick.ContainsKey(player))
        {
            _playersNextTick.Remove(player);
        }
    }

    void Update()
    {
        if (!periodicDamage || _playersNextTick.Count == 0)
            return;

        // Copy keys to avoid modifying collection while iterating
        var players = new List<PlayerCharacter>(_playersNextTick.Keys);
        foreach (var player in players)
        {
            if (player == null)
            {
                _playersNextTick.Remove(player);
                continue;
            }

            if (Time.time >= _playersNextTick[player])
            {
                player.TakeDamage(damagePerTick);
                _playersNextTick[player] = Time.time + Mathf.Max(0.01f, tickInterval);
            }
        }
    }

    // Optional API to change parameters at runtime
    public void SetInstantDamage(float amount, bool enabled)
    {
        instantDamageAmount = amount;
        instantDamageOnEnter = enabled;
    }

    public void SetPeriodicDamage(float amount, float interval, bool enabled, bool tickImmediately = false)
    {
        damagePerTick = amount;
        tickInterval = Mathf.Max(0.01f, interval);
        periodicDamage = enabled;
        tickOnEnter = tickImmediately;
    }
}
