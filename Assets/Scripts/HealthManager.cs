using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        // Start everyone at full health
        currentHealth = maxHealth;
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
            Destroy(gameObject);
        }
    }
}