using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapon References")]
    [Tooltip("GunAttack component for the Gun weapon.")]
    [SerializeField] private GunAttack gunAttack;

    [Tooltip("GunAttack component for the Rifle weapon.")]
    [SerializeField] private GunAttack rifleAttack;

    private enum ActiveWeapon { None, Gun, Rifle }
    private ActiveWeapon currentWeapon = ActiveWeapon.None;

    // Track which weapon was active before hammer interrupted
    private ActiveWeapon weaponBeforeHammer = ActiveWeapon.None;

    void Start()
    {
        // Deactivate all first, then activate default weapon
        DeactivateAll();
        OnSelectGun();
    }

    /// <summary>
    /// Called by UI Button "Gun".
    /// </summary>
    public void OnSelectGun()
    {
        DeactivateAll();
        currentWeapon = ActiveWeapon.Gun;

        if (gunAttack != null)
        {
            gunAttack.ActivateWeapon();
        }
    }

    /// <summary>
    /// Called by UI Button "Rifle".
    /// </summary>
    public void OnSelectRifle()
    {
        DeactivateAll();
        currentWeapon = ActiveWeapon.Rifle;

        if (rifleAttack != null)
        {
            rifleAttack.ActivateWeapon();
        }
    }

    /// <summary>
    /// Called by HammerAttack at the START of hammer swing.
    /// Remembers which weapon was active and disables it.
    /// </summary>
    public void DisableForHammer()
    {
        weaponBeforeHammer = currentWeapon;

        switch (currentWeapon)
        {
            case ActiveWeapon.Gun:
                if (gunAttack != null)
                    gunAttack.DisableForHammer();
                break;

            case ActiveWeapon.Rifle:
                if (rifleAttack != null)
                    rifleAttack.DisableForHammer();
                break;
        }
    }

    /// <summary>
    /// Called by HammerAttack.EndAttack() when hammer is fully finished.
    /// Restores the weapon that was active before the hammer interrupted.
    /// </summary>
    public void ReEnableAfterHammer()
    {
        switch (weaponBeforeHammer)
        {
            case ActiveWeapon.Gun:
                if (gunAttack != null)
                    gunAttack.ReEnableAfterHammer();
                break;

            case ActiveWeapon.Rifle:
                if (rifleAttack != null)
                    rifleAttack.ReEnableAfterHammer();
                break;
        }

        weaponBeforeHammer = ActiveWeapon.None;
    }

    /// <summary>
    /// Deactivates all ranged weapons.
    /// </summary>
    public void DeactivateAll()
    {
        if (gunAttack != null)
            gunAttack.DeactivateWeapon();

        if (rifleAttack != null)
            rifleAttack.DeactivateWeapon();

        currentWeapon = ActiveWeapon.None;
        weaponBeforeHammer = ActiveWeapon.None;
    }
}