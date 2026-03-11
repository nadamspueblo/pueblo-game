using UnityEngine;

public class DamageFeedback : MonoBehaviour
{
    [Header("Animations")]
    public Animator anim;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip takeDamageSound;
    public AudioClip deathSound;

    void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // We will link this to the HealthManager's OnTakeDamage event
    public void PlayHitReaction() 
    {
        if (anim != null) anim.SetTrigger("Hit");
        
        if (audioSource != null && takeDamageSound != null) 
        {
            audioSource.PlayOneShot(takeDamageSound);
        }
    }

    // We will link this to the HealthManager's OnDeath event
    public void PlayDeathReaction() 
    {
        if (anim != null) anim.SetTrigger("Death");
        
        if (audioSource != null && deathSound != null) 
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // This is also a great place to disable the zombie's AI and NavMeshAgent 
        // so it stops chasing you while the death animation plays!
    }
}