using UnityEngine;
using StarterAssets;

public class PlayerCombatState : MonoBehaviour
{
  [Header("Core References")]
  public ThirdPersonController tpc;
  public StarterAssetsInputs input;

  [Header("Equipment")]
  public Transform weaponHolderTransform;
  public GameObject currentWeaponPrefab; // Track the instantiated object so we can destroy it later

  [Header("The Single Source of Truth")]
  public CombatStance currentStance = CombatStance.Standard;
  public WeaponType currentWeaponType = WeaponType.Unarmed;

  void Start()
  {
    if (tpc == null) tpc = GetComponent<ThirdPersonController>();
    if (input == null) input = GetComponent<StarterAssetsInputs>();

    // Safety check to find the hand bone if not assigned
    if (weaponHolderTransform == null)
    {
      GameObject holder = GameObject.FindWithTag("WeaponHolder");
      if (holder != null) weaponHolderTransform = holder.transform;
    }

    if (currentWeaponPrefab != null) currentWeaponPrefab.SetActive(false);
  }

  void Update()
  {
    if (tpc == null || input == null) return;

    // Handle the crouch toggle first
    if (input.crouch)
    {
        // If we are sneaking, exit. If we aren't, enter sneak.
        currentStance = (currentStance == CombatStance.Sneak) ? 
                        (input.aim ? CombatStance.Combat : CombatStance.Standard) : 
                        CombatStance.Sneak;
        input.crouch = false; 
    }
    // If we aren't locked in a crouch, constantly evaluate the aim button
    else if (currentStance != CombatStance.Sneak)
    {
        currentStance = input.aim ? CombatStance.Combat : CombatStance.Standard;
    }

    // Make sure we're in the right movement state
    // Remember: ChangeState only changes if it's a new state
    switch (currentStance)
    {
        case CombatStance.Standard:
            tpc.ChangeState(PlayerMovementState.FreeExplore);
            input.lightAttack = false;
            input.heavyAttack = false;
            input.specialAttack = false;
            input.block = false;
            break;

        case CombatStance.Combat:
            tpc.ChangeState(PlayerMovementState.CombatStrafe);
            input.jump = false; // No jumping while aiming
            break;

        case CombatStance.Sneak:
            tpc.ChangeState(PlayerMovementState.Sneak);
            input.lightAttack = false;
            input.heavyAttack = false;
            input.specialAttack = false;
            input.block = false;
            input.jump = false; // No jumping while sneaking
            break;
    }
  }

  public void EquipWeapon(ItemData weaponItem)
  {
    // 1. Clean up the old weapon if we already have one
    if (currentWeaponPrefab != null)
    {
      Destroy(currentWeaponPrefab);
    }

    // 2. Instantiate the new weapon and set its parent to the hand
    currentWeaponPrefab = Instantiate(weaponItem.itemPrefab, weaponHolderTransform, false);
    currentWeaponType = weaponItem.weaponType;

    // 3. Wire up the hit detection to our Relay script
    PlayerMeleeAttack attackRelay = GetComponent<PlayerMeleeAttack>();
    if (attackRelay != null)
    {
      attackRelay.weaponHitbox = currentWeaponPrefab.GetComponent<WeaponHitbox>();
    }

    // 4. Tell the Animator to switch idle poses
    Animator anim = GetComponent<Animator>();
    if (anim != null)
    {
      anim.SetInteger("WeaponType", (int)weaponItem.weaponType);
      anim.SetTrigger("SwitchWeapon");
    }
  }

  // --- ANIMATION EVENTS ---
  public void Event_DrawWeapon()
  {
    if (currentWeaponPrefab != null) currentWeaponPrefab.SetActive(true);
  }

  public void Event_HolsterWeapon()
  {
    if (currentWeaponPrefab != null) currentWeaponPrefab.SetActive(false);
  }
}