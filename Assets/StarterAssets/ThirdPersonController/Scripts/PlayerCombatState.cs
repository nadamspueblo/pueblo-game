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

    // 1. Determine the Stance based on input
    // (Assuming you have a crouch/sneak boolean in your Input system)
    /* if (input.crouch) 
    {
        currentStance = CombatStance.Sneak;
    }
    else 
    */
    if (input.aim)
    {
      currentStance = CombatStance.Combat;
    }
    else
    {
      currentStance = CombatStance.Standard;
    }

    // 2. Tell the Movement Controller if we are in a combat-ready state
    tpc.isCombatMode = (currentStance == CombatStance.Combat || currentStance == CombatStance.Sneak);

    // 3. Prevent rogue attacks if we are just walking around in Standard stance
    if (currentStance == CombatStance.Standard)
    {
      input.lightAttack = false;
      input.heavyAttack = false;
      input.specialAttack = false;
      input.block = false;
    }
    else if (currentStance == CombatStance.Combat)
    {
      input.jump = false;
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