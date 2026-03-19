using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnAttackButtonDown();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnAttackButtonUp();
        }
    }
}