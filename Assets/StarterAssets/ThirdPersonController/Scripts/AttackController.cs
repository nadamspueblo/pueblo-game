using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public enum CombatStance { Standard, Combat, Sneak } // Updated!
public enum WeaponType { Unarmed, Melee1Hand, Melee2Hand }
public enum AttackInputType { Light, Heavy, Special }

[System.Serializable]
public struct AttackDefinition
{
  public string attackName; // e.g., "1H Light 1", "2H Heavy Finisher"

  [Header("State Requirements")]
  public WeaponType requiredWeapon;
  public CombatStance requiredStance;
  public AttackInputType requiredInput;
  public float staminaCost;

  [Tooltip("0 = First hit, 1 = Second hit, 2 = Third hit")]
  public int comboStep;

  [Header("Animation & Polish")]
  public string animatorTrigger;
  public float rootTwistCorrection;
}

public class AttackController : MonoBehaviour
{
  [Header("Core References")]
  public Animator playerAnimator;
  public ThirdPersonController tpc;
  public StarterAssetsInputs input;
  public CombatMagnetism combatMagnetism;
  public SurvivalStats survivalStats;
  public PlayerCombatState combatState;

  [Header("Stamina Usage")]
  public float blockStamina = 20f;
  public float breakGrabStamina = 40f;

  [Header("Spine Correction")]
  public Transform spineBone;
  public Vector3 twistAxis = new Vector3(0, 1, 0);
  private float currentTwist = 0f;
  private float targetTwist = 0f;

  [Header("Combo System")]
  public List<AttackDefinition> attackMoveset = new List<AttackDefinition>();
  public float comboResetTime = 1.5f;

  private int currentComboStep = 0;
  private float lastAttackTime = 0f;
  private bool isAttacking = false;
  private bool hasBufferedInput = false;
  private AttackInputType bufferedInputType;
  public bool isBlocking = false;

  [Header("Safety Failsafes")]
  public float maxAttackDuration = 3.5f;

  void Start()
  {
    if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
    if (tpc == null) tpc = GetComponent<ThirdPersonController>();
    if (input == null) input = GetComponent<StarterAssetsInputs>();
    if (combatState == null) combatState = GetComponent<PlayerCombatState>();
    if (survivalStats == null) survivalStats = GetComponent<SurvivalStats>();
  }

  void Update()
  {
    if (isAttacking && (Time.time - lastAttackTime > maxAttackDuration))
    {
      Debug.LogWarning("Attack Animation Event missed! Forcing state reset.");
      CancelAttack();
    }

    if (currentComboStep > 0 && Time.time - lastAttackTime > comboResetTime && !isAttacking)
    {
      ResetCombo();
    }

    // We only allow attacking and blocking if we are NOT in the Standard stance
    if (combatState != null && combatState.currentStance != CombatStance.Standard)
    {
      HandleBlocking();
      HandleAttackInputs();
    }
    else if (isBlocking)
    {
      SetBlocking(false);
    }
  }

  void LateUpdate()
  {
    if (spineBone == null) return;
    currentTwist = Mathf.Lerp(currentTwist, targetTwist, Time.deltaTime * 10f);
    if (Mathf.Abs(currentTwist) > 0.1f)
    {
      spineBone.Rotate(twistAxis, currentTwist, Space.Self);
    }
  }

  private void HandleBlocking()
  {
    if (input.block != isBlocking) SetBlocking(input.block);
    //if (isBlocking) CancelAttack();
  }

  private void SetBlocking(bool state)
  {
    isBlocking = state;
    playerAnimator.SetBool("IsBlocking", isBlocking);
  }

  private void HandleAttackInputs()
  {
    if (isBlocking)
    {
      input.lightAttack = false;
      input.heavyAttack = false;
      input.specialAttack = false;
      return;
    }
    bool attackPressed = false;
    AttackInputType requestedInput = AttackInputType.Light;

    if (input.lightAttack) { attackPressed = true; requestedInput = AttackInputType.Light; input.lightAttack = false; }
    else if (input.heavyAttack) { attackPressed = true; requestedInput = AttackInputType.Heavy; input.heavyAttack = false; }
    else if (input.specialAttack) { attackPressed = true; requestedInput = AttackInputType.Special; input.specialAttack = false; }

    if (attackPressed)
    {
      if (!isAttacking) ExecuteAttack(requestedInput);
      else
      {
        hasBufferedInput = true;
        bufferedInputType = requestedInput;
      }
    }
  }

  private void ExecuteAttack(AttackInputType requestedInput)
  {
    // Read directly from the State Manager to find the correct attack!
    AttackDefinition? matchedAttack = FindAttackDefinition(
        combatState.currentWeaponType,
        combatState.currentStance,
        requestedInput,
        currentComboStep
    );

    if (!matchedAttack.HasValue && currentComboStep > 0)
    {
      matchedAttack = FindAttackDefinition(combatState.currentWeaponType, combatState.currentStance, requestedInput, 0);
      if (matchedAttack.HasValue) currentComboStep = 0;
    }

    if (matchedAttack.HasValue && survivalStats.UseStamina(matchedAttack.Value.staminaCost))
    {
      isAttacking = true;
      lastAttackTime = Time.time;
      targetTwist = matchedAttack.Value.rootTwistCorrection;
      playerAnimator.SetTrigger(matchedAttack.Value.animatorTrigger);
      if (combatMagnetism != null) combatMagnetism.TriggerMagnetism();
      currentComboStep++;
    }
    else if (!matchedAttack.HasValue)
    {
      // Debugging will now tell you exactly what combination is missing from your Inspector list!
      Debug.LogWarning($"No attack mapped for {combatState.currentWeaponType} | {combatState.currentStance} | {requestedInput} | Step {currentComboStep}");
    }
  }

  private AttackDefinition? FindAttackDefinition(WeaponType type, CombatStance stance, AttackInputType inputType, int step)
  {
    foreach (var attack in attackMoveset)
    {
      if (attack.requiredWeapon == type && attack.requiredStance == stance && attack.requiredInput == inputType && attack.comboStep == step)
      {
        return attack;
      }
    }
    return null;
  }

  public void CancelAttack()
  {
    if (!isAttacking && currentComboStep == 0) return;
    isAttacking = false;
    hasBufferedInput = false;
    targetTwist = 0f;
    currentComboStep = 0;
  }

  public void EndAttack()
  {
    isAttacking = false;
    targetTwist = 0f;
    if (hasBufferedInput)
    {
      hasBufferedInput = false;
      ExecuteAttack(bufferedInputType);
    }
  }

  private void ResetCombo()
  {
    currentComboStep = 0;
    targetTwist = 0f;
  }
}