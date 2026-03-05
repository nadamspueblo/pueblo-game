using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator playerAnimator; 
    public string swingTrigger = "SwingAttack";
    
    [Header("Input Settings")]
    public KeyCode attackKey = KeyCode.Mouse0;
    
    [Header("Timing Settings")]
    public float attackCooldown = 2.0f; // Adjust based on your animation length
    
    private bool canAttack = true;
    
    void Start()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInParent<Animator>();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(attackKey) && canAttack)
        {
            SwingWeapon();
        }
    }
    
    void SwingWeapon()
    {
        if (playerAnimator != null)
        {
            // Trigger the swing animation
            playerAnimator.SetTrigger(swingTrigger);
            
            // Prevent spam clicking
            canAttack = false;
            
            // Reset after the animation should be done
            Invoke("ResetAttack", attackCooldown);
        }
    }
    
    void ResetAttack()
    {
        canAttack = true;
    }
}
