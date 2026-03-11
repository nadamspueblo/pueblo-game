using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Animations")]
    public Animator anim;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip takeDamageSound;
    public AudioClip deathSound;

    void Start()
    {
        // Start everyone at full health
        currentHealth = maxHealth;
        if (anim == null) {
            anim = GetComponent<Animator>();
        }
        if (audioSource == null) {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }

    // This is public so the weapon can call it
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took damage! Health is now: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else {
            PlayHitReaction();
            PlayHitSound();
        }
    }

    public bool IsDead() {
        return currentHealth <= 0;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        
        // If it's the player, we'll eventually trigger a Game Over screen
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("GAME OVER!");
        }
        else // If it's a zombie, destroy it so it disappears from the scene
        {
            //Destroy(gameObject);
            PlayDeath();
            
        }
    }

    private void PlayHitReaction() {
        if (anim != null) {
            anim.SetTrigger("Hit");
        }
    }

    private void PlayHitSound() {
        if (audioSource != null && takeDamageSound != null) {
            audioSource.PlayOneShot(takeDamageSound);
        }
    }

    private void PlayDeath() {
        if (anim != null) {
            anim.SetTrigger("Death");
            PlayDeathSound();
        }
        else {
            Destroy(gameObject);
        }
    }

    private void PlayDeathSound() {
        if (audioSource != null && deathSound != null) {
            audioSource.PlayOneShot(deathSound);
        }
    }
}