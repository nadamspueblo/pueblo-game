using UnityEngine;

public class PlayerCombatReactions : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;
    public WeaponController weaponController;
    public SurvivalStats stats;

    void Start()
    {
        // Tune the radio! When SurvivalStats broadcasts a hit, run our ReactToHit function.
        if (stats != null)
        {
            stats.onTakeDamage.AddListener(ReactToHit);
        }
    }

    // This function automatically runs whenever the event fires
    public void ReactToHit(float damage, Transform attacker)
    {
        if (playerAnimator == null || attacker == null) return;

        // 1. Calculate the normalized direction vector to the attacker
        Vector3 dirToAttacker = (attacker.position - transform.position).normalized;
        
        // 2. Calculate the Dot Products for our 2D Blend Tree
        float hitZ = Vector3.Dot(transform.forward, dirToAttacker); // Front/Back
        float hitX = Vector3.Dot(transform.right, dirToAttacker);   // Right/Left

        // 3. Are we blocking?
        if (weaponController != null && weaponController.isBlocking)
        {
            // Check if the attack is actually coming from the front!
            if (hitZ > 0.5f)
            {
                playerAnimator.SetTrigger("Hit"); // Triggers the Upper Body block flinch
                return; // Stop here so we don't play the full body stagger!
            }
        }

        // 4. We got hit! Send the math to the Animator for the Full Body override
        playerAnimator.SetFloat("HitX", hitX);
        playerAnimator.SetFloat("HitZ", hitZ);
        playerAnimator.SetTrigger("Hit"); 
    }
}