using UnityEngine;
using UnityEngine.Events; // Required for events!

public class HealthManager : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent<float, Transform> onTakeDamage;
    public UnityEvent onDeath;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return; // Prevent taking damage after death

        currentHealth -= damage;
        Debug.Log(gameObject.name + " took damage! Health is now: " + currentHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            Debug.Log(gameObject.name + " has died!");
            onDeath?.Invoke(); // Shout to the game that this object died!
        }
        else 
        {
            onTakeDamage?.Invoke(damage, attacker); // Shout that we got hit!
        }
    }

    public bool IsDead() 
    {
        return isDead;
    }
}