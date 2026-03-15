using UnityEngine;
using StarterAssets;

public class PlayerCombatState : MonoBehaviour
{
  [Header("References")]
  public ThirdPersonController tpc;
  public StarterAssetsInputs input;

  void Start()
  {
    if (tpc == null) tpc = GetComponent<ThirdPersonController>();
    if (input == null) input = GetComponent<StarterAssetsInputs>();
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
}