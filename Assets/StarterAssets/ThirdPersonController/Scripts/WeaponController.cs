using UnityEngine;
using StarterAssets;

public class WeaponController : MonoBehaviour
{
    [Header("Core References")]
    public Animator playerAnimator;
    public ThirdPersonController tpc; 
    public StarterAssetsInputs input; // NEW: Reference to the Input System

    [Header("Combat Settings")]
    public string attackTrigger = "Attack";
    public string attackTypeInt = "AttackType"; // 0=Light, 1=Heavy, 2=Special
    public float defaultAttackCooldown = 1.5f;

    [Header("Spine Correction (Per Attack)")]
    public Transform spineBone;
    public Vector3 twistAxis = new Vector3(0, 1, 0);
    [Tooltip("Set this to 0 if the Axe Pack animations already aim perfectly!")]
    public float lightAttackTwist = -60f; 
    public float heavyAttackTwist = 0f;
    public float specialAttackTwist = 0f;

    private bool canAttack = true;
    private float currentTwist = 0f;
    private float targetTwist = 0f;

    public bool isBlocking = false;

    void Start()
    {
        if (playerAnimator == null) playerAnimator = GetComponentInParent<Animator>();
        if (tpc == null) tpc = GetComponentInParent<ThirdPersonController>();
        if (input == null) input = GetComponentInParent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (tpc != null && tpc.isCombatMode)
        {
            // 1. Handle Blocking via Input System
            if (input.block)
            {
                isBlocking = true;
                playerAnimator.SetBool("IsBlocking", true);
            }
            else
            {
                isBlocking = false;
                playerAnimator.SetBool("IsBlocking", false);
            }

            // 2. Handle Attacks (Only if NOT blocking and cooldown is finished)
            if (canAttack)
            {
                if (input.lightAttack)
                {
                    ExecuteAttack(0, lightAttackTwist);
                    input.lightAttack = false; // Consume the input so it doesn't double-fire
                }
                else if (input.heavyAttack)
                {
                    ExecuteAttack(1, heavyAttackTwist);
                    input.heavyAttack = false;
                }
                else if (input.specialAttack)
                {
                    ExecuteAttack(2, specialAttackTwist);
                    input.specialAttack = false;
                }
            }
        }
        else if (isBlocking)
        {
            // Failsafe: Drop shield if we exit combat mode
            isBlocking = false;
            playerAnimator.SetBool("IsBlocking", false);
        }
    }

    void LateUpdate()
    {
        if (spineBone == null) return;

        // Smoothly twist to the requested angle for this specific attack
        currentTwist = Mathf.Lerp(currentTwist, targetTwist, Time.deltaTime * 10f);

        if (Mathf.Abs(currentTwist) > 0.1f)
        {
            spineBone.Rotate(twistAxis, currentTwist, Space.Self);
        }
    }

    // This handles all 3 attack types dynamically!
    void ExecuteAttack(int attackType, float requiredTwist)
    {
        if (playerAnimator != null)
        {
            // Tell the Animator which attack to play, then pull the trigger
            playerAnimator.SetInteger(attackTypeInt, attackType);
            playerAnimator.SetTrigger(attackTrigger);
            
            // Set our target twist for the LateUpdate
            targetTwist = requiredTwist;
            canAttack = false;
            
            // Start the cooldown
            Invoke("ResetAttack", defaultAttackCooldown);
        }
    }

    void ResetAttack()
    {
        canAttack = true;
        targetTwist = 0f; // Untwist the spine when the attack finishes
    }
}