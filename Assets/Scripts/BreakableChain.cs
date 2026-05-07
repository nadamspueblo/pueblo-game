using UnityEngine;
using UnityEngine.Events;

public class BreakableChain : MonoBehaviour
{
    [Header("Chain Health")]
    [SerializeField] private float maxHealth = 50f; // How much damage needed to break the chain
    private float currentHealth;

    [Header("Connected Gate")]
    [SerializeField] private SlidingGate connectedGate; // Reference to the gate this chain locks

    [Header("Effects")]
    [SerializeField] private AudioClip breakSound; // Sound when chain breaks
    [SerializeField] private GameObject breakParticles; // Optional: sparks/metal particles

    [Header("Events")]
    public UnityEvent onChainBroken; // Fires when chain breaks

    private AudioSource audioSource;
    private bool isBroken = false;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        // If no AudioSource exists, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // Make it 3D sound
        }
    }

    // This gets called by WeaponHitbox when the player hits the chain
    public void TakeDamage(float damageAmount)
    {
        if (isBroken) return; // Already broken, ignore further hits

        currentHealth -= damageAmount;

        Debug.Log($"Chain took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Check if chain should break
        if (currentHealth <= 0)
        {
            BreakChain();
        }
    }

    void BreakChain()
    {
        isBroken = true;

        // Play break sound
        if (breakSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // Spawn particle effect
        if (breakParticles != null)
        {
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        }

        // Tell the gate it can open now
        if (connectedGate != null)
        {
            connectedGate.UnlockGate();
        }

        // Fire the UnityEvent (in case you want to trigger other things)
        onChainBroken?.Invoke();

        // Optional: Make chain links scatter with physics
        // Uncomment these lines for dramatic effect:
        /*
        Rigidbody[] linkRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in linkRigidbodies)
        {
            rb.AddExplosionForce(500f, transform.position, 5f);
        }
        Destroy(gameObject, 2f); // Give links time to scatter
        */

        // Destroy the chain after a short delay (so sound plays)
        Destroy(gameObject, 0.5f);
    }
}

