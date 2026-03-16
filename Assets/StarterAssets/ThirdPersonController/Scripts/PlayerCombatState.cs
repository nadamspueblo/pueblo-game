using UnityEngine;
using StarterAssets;
using System.Data.SqlTypes;

public class PlayerCombatState : MonoBehaviour
{
  [Header("References")]
  public ThirdPersonController tpc;
  public StarterAssetsInputs input;
  public Transform weaponHolderTransform;
  public GameObject weapon;
  public WeaponType weaponType = WeaponType.Unarmed;

  void Start()
  {
    if (tpc == null) tpc = GetComponent<ThirdPersonController>();
    if (input == null) input = GetComponent<StarterAssetsInputs>();
    if (weaponHolderTransform == null) weaponHolderTransform = GameObject.FindWithTag("WeaponHolder").transform;
    if (weapon != null) weapon.SetActive(false);
  }

  void Update()
  {
    if (tpc != null && input != null)
    {
      // If holding right-click, tell the movement script to enter combat mode!
      tpc.isCombatMode = input.aim;
    }

    if (!tpc.isCombatMode)
    {
      input.lightAttack = false;
      input.block = false;
      input.heavyAttack = false;
      input.specialAttack = false;
    }
  }

  public void EquipWeapon(ItemData weaponItem)
  {
    // Instantiate the weapon and set its parent
    weapon = Instantiate(weaponItem.itemPrefab, weaponHolderTransform, false);
    
    // Update attack references
    PlayerMeleeAttack attack = GetComponent<PlayerMeleeAttack>();
    attack.weaponHitbox = weapon.GetComponent<WeaponHitbox>();
    weaponType = weaponItem.weaponType;
    
    // Animate unequipping the weapon
    Animator anim = GetComponent<Animator>();
    if (anim != null) {
      anim.SetInteger("WeaponType", (int)weaponItem.weaponType);
      anim.SetTrigger("SwitchWeapon");
    }
  }

  public void Event_DrawWeapon()
  {
    if (weapon != null)
    {
      weapon.SetActive(true);
    }
  }

  public void Event_HolsterWeapon()
  {
    if (weapon != null)
    {
      weapon.SetActive(false);
    }
  }
}