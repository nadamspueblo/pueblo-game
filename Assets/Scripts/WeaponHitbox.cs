using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    public float damageAmount = 25f;
    private Collider hitboxCollider;

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        // Start the game with the weapon turned OFF so we don't deal accidental damage
        hitboxCollider.enabled = false; 
    }

    // The Animation Event will call this to turn the weapon ON
    public void EnableHitbox()
    {
        hitboxCollider.enabled = true;
    }

    // The Animation Event will call this to turn the weapon OFF
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Make sure we don't accidentally hit ourselves
        if (other.transform.root == transform.root) return;

        // 2. Check if the thing we hit has health
        HealthManager targetHealth = other.GetComponent<HealthManager>();
        
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageAmount);
            
            // 3. Immediately turn off the hitbox so it doesn't hit the zombie 5 times in one frame
            DisableHitbox(); 
        }
    }
}