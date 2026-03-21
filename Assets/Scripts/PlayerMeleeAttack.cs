using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
  public WeaponHitbox weaponHitbox; // Drag the player's weapon hitbox here
  public WeaponHitbox rightHandHitbox;
  public WeaponHitbox leftHandHitbox;
  public WeaponHitbox rightFootHitbox;

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

  public void Event_EnableRightPunch()
  {
    if (rightHandHitbox != null) rightHandHitbox.EnableHitbox();
  }

  public void Event_DisableRightPunch()
  {
    if (rightHandHitbox != null) rightHandHitbox.DisableHitbox();
  }

  public void Event_EnableLeftPunch()
  {
    if (rightHandHitbox != null) leftHandHitbox.EnableHitbox();
  }

  public void Event_DisableLeftPunch()
  {
    if (rightHandHitbox != null) leftHandHitbox.DisableHitbox();
  }

  public void Event_EnableRightKick()
  {
    if (rightFootHitbox != null) rightFootHitbox.EnableHitbox();
  }

  public void Event_DisableRightKick()
  {
    if (rightFootHitbox != null) rightFootHitbox.DisableHitbox();
  }
}