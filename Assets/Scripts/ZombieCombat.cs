using UnityEngine;

public class ZombieCombat : MonoBehaviour
{
    public WeaponHitbox zombieHitbox; // Drag the ZombieBiteHitbox here!

    // The Animation Event will call this when the arm swings forward
    public void Event_EnableWeapon()
    {
        if (zombieHitbox != null) zombieHitbox.EnableHitbox();
    }

    // The Animation Event will call this when the swing finishes
    public void Event_DisableWeapon()
    {
        if (zombieHitbox != null) zombieHitbox.DisableHitbox();
    }
}