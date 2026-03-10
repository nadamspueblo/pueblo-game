using UnityEngine;
using StarterAssets; // Needed to access the ThirdPersonController

public class WeaponController : MonoBehaviour
{
    [Header("Core References")]
    public Animator playerAnimator; 
    public ThirdPersonController tpc; // Drag the Player here
    public SurvivalStats playerStats; // Drag the Player here

    [Header("Animation Settings")]
    public string swingTrigger = "SwingAttack";
    
    [Header("Input Settings")]
    public KeyCode aimKey = KeyCode.Mouse1;   // Right Click to ready weapon
    public KeyCode attackKey = KeyCode.Mouse0; // Left Click to swing
    
    [Header("Combat Settings")]
    public float attackCooldown = 1.5f; 
    public float attackStaminaCost = 25f;
    
    private bool canAttack = true;
    
    void Start()
    {
        if (playerAnimator == null) playerAnimator = GetComponentInParent<Animator>();
        if (tpc == null) tpc = GetComponentInParent<ThirdPersonController>();
        if (playerStats == null) playerStats = GetComponentInParent<SurvivalStats>();
    }
    
    void Update()
    {
        // 1. Hold Right-Click to enter Combat/Strafe Mode
        if (Input.GetKey(aimKey))
        {
            if (tpc != null) tpc.isCombatMode = true;

            // 2. While aiming, Left-Click to swing (if they have stamina!)
            if (Input.GetKeyDown(attackKey) && canAttack)
            {
                if (playerStats != null && playerStats.UseStamina(attackStaminaCost))
                {
                    SwingWeapon();
                }
            }
        }
        else
        {
            // Releasing Right-Click drops them back into normal exploration movement
            if (tpc != null) tpc.isCombatMode = false;
        }
    }
    
    void SwingWeapon()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(swingTrigger);
            canAttack = false;
            Invoke("ResetAttack", attackCooldown);
        }
    }
    
    void ResetAttack()
    {
        canAttack = true;
    }
}