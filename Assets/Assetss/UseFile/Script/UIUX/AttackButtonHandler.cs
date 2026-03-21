using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler
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

    public void OnPointerExit(PointerEventData eventData)
    {
        // When finger slides off the button, treat as release
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnAttackButtonUp();
        }
    }

    // Required: implementing IDragHandler (even empty) prevents Unity from
    // cancelling the pointer when the finger moves slightly on mobile touch.
    public void OnDrag(PointerEventData eventData)
    {
        // Intentionally empty — keeps the pointer "alive" during hold
    }
}