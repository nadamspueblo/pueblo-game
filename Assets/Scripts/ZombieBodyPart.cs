using UnityEngine;

public class ZombieBodyPart : MonoBehaviour
{
    public enum PartType { Head, Torso, Legs }
    public PartType bodyPart;
    
    [Header("Local Stats")]
    public float localHealth = 100f;
    public float damageMultiplier = 1.0f;
    private bool isBroken = false; // Prevents triggering "crippled" effects multiple times

    [Header("References")]
    public HealthManager mainHealth;
    public DamageFeedback damageFeedback; // Direct link to your VFX/Anim script!

    public void HitByWeapon(float baseDamage, Transform attackerWeapon, Transform attackerRoot)
    {
        // 1. Calculate specific hit location for blood
        Collider myCollider = GetComponent<Collider>();
        Vector3 exactHitPoint = myCollider.ClosestPoint(attackerWeapon.position);

        // 2. Calculate the damage
        float calculatedDamage = baseDamage * damageMultiplier;
         Debug.Log(bodyPart.ToString() + " took damage!");

        // 3. Deduct Local Health
        if (!isBroken)
        {
            localHealth -= calculatedDamage;
            if (localHealth <= 0)
            {
                isBroken = true;
                // Optional: Trigger a permanent crippling effect here!
                Debug.Log(bodyPart.ToString() + " has been crippled!");
            }
        }

        // 4. Tell DamageFeedback to play the specific reaction and spawn blood
        if (damageFeedback != null)
        {
            damageFeedback.PlayLocationalReaction(bodyPart, exactHitPoint, attackerRoot, this.transform);
        }

        // 5. Forward the damage to the main brain (Optional based on your game design!)
        if (isBroken && mainHealth != null)
        {
            mainHealth.TakeDamage(calculatedDamage, attackerRoot);
        }
    }
}