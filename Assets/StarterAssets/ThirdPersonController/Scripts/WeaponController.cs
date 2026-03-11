using UnityEngine;
using StarterAssets; 

public class WeaponController : MonoBehaviour
{
    [Header("Core References")]
    public Animator playerAnimator; 
    public ThirdPersonController tpc; // We need this to check if we are aiming!
    
    [Header("Animation Settings")]
    public string swingTrigger = "SwingAttack";
    
    [Header("Input Settings")]
    public KeyCode attackKey = KeyCode.Mouse0; // Left Click
    
    [Header("Timing Settings")]
    public float attackCooldown = 1.5f; 
    
    [Header("Spine Correction")]
    public Transform spineBone; 
    public float twistAngle = 45f;
    public Vector3 twistAxis = new Vector3(0, 1, 0); 
    
    private bool canAttack = true;
    private float currentTwist = 0f;
    
    void Start()
    {
        if (playerAnimator == null) playerAnimator = GetComponentInParent<Animator>();
        if (tpc == null) tpc = GetComponentInParent<ThirdPersonController>();
    }
    
    void Update()
    {
        // SRP IN ACTION: We only swing if we are allowed to attack AND the player is aiming
        if (tpc != null && tpc.isCombatMode)
        {
            if (Input.GetKeyDown(attackKey) && canAttack)
            {
                SwingWeapon();
            }
        }
    }
    
    void LateUpdate()
    {
        if (spineBone == null) return;

        float targetTwist = canAttack ? 0f : twistAngle;
        currentTwist = Mathf.Lerp(currentTwist, targetTwist, Time.deltaTime * 10f);

        if (Mathf.Abs(currentTwist) > 0.1f)
        {
            spineBone.Rotate(twistAxis, currentTwist, Space.Self);
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