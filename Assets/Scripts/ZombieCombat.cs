using UnityEngine;

public class ZombieCombat : MonoBehaviour
{
    public WeaponHitbox rightHandHitbox; // Drag the ZombieBiteHitbox here!
    public WeaponHitbox leftHandHitbox;

    // The Animation Event will call this when the arm swings forward
    public void Event_EnableRightHandHitbox()
    {
        if (rightHandHitbox != null) rightHandHitbox.EnableHitbox();
    }

    // The Animation Event will call this when the swing finishes
    public void Event_DisableRightHandHitbox()
    {
        if (rightHandHitbox != null) rightHandHitbox.DisableHitbox();
    }

    public void Event_EnableLeftHandHitbox()
    {
        if (leftHandHitbox != null) leftHandHitbox.EnableHitbox();
    }

    // The Animation Event will call this when the swing finishes
    public void Event_DisableLeftHandHitbox()
    {
        if (leftHandHitbox != null) leftHandHitbox.DisableHitbox();
    }
}