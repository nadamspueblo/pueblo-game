using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public WeaponHitbox weaponHitbox; // Drag the ZombieBiteHitbox here!

    // The Animation Event will call this when the arm swings forward
    public void Event_EnableWeapon()
    {
        if (weaponHitbox != null) weaponHitbox.EnableHitbox();
    }

    // The Animation Event will call this when the swing finishes
    public void Event_DisableWeapon()
    {
        if (weaponHitbox != null) weaponHitbox.DisableHitbox();
    }
}