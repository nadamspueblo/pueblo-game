using UnityEngine;
using UnityEngine.Events; // This lets us easily update the UI later!

public class SurvivalStats : MonoBehaviour
{
    [Header("Stat Maximums")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxSleep = 100f;
    public float maxStamina = 100f;

    [Header("Current Stats")]
    public float currentHealth;
    public float currentHunger;
    public float currentThirst;
    public float currentSleep;
    public float currentStamina;

    [Header("Depletion Rates (Per Second)")]
    public float hungerDrainRate = 0.5f;
    public float thirstDrainRate = 1f; // Thirst usually drops faster than hunger
    public float sleepDrainRate = 0.2f;

    [Header("Stamina Settings")]
    public float staminaRegenRate = 15f; 

    [Header("Events")]
    // We will use these later to tell the UI Canvas to update its bars
    public UnityEvent onStatsChanged; 
    public UnityEvent onPlayerDeath;

    void Start()
    {
        // 1. Initialize all stats to their maximums when the game begins
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentSleep = maxSleep;
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandlePassiveDrain();
        HandleStaminaRegen();
    }

    private void HandlePassiveDrain()
    {
        // 2. Drain stats over time. 
        // Multiplying by Time.deltaTime ensures it drains per second, not per frame!
        currentHunger -= hungerDrainRate * Time.deltaTime;
        currentThirst -= thirstDrainRate * Time.deltaTime;
        currentSleep -= sleepDrainRate * Time.deltaTime;

        // 3. Clamp values so they never drop below 0 or exceed the maximum
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);
        currentSleep = Mathf.Clamp(currentSleep, 0, maxSleep);

        // 4. The Consequences! If starving or dehydrated, slowly drain health
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(2f * Time.deltaTime); 
        }

        // Fire off a message that stats have changed for UI update
        onStatsChanged?.Invoke();
    }

    private void HandleStaminaRegen()
    {
        // Stamina naturally regenerates over time if it isn't full
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * (currentHunger + currentSleep + currentThirst) / (maxHunger + maxSleep + maxThirst) * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            onStatsChanged?.Invoke();
        }
    }

    // --- PUBLIC METHODS FOR OTHER SCRIPTS TO USE ---

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        if (currentHealth <= 0)
        {
            Debug.Log("Player has died!");
            onPlayerDeath?.Invoke();
        }
        onStatsChanged?.Invoke();
    }

    // Use this when eating food, drinking water, or sleeping
    public void RestoreStat(string statName, float amount)
    {
        switch (statName.ToLower())
        {
            case "health":
                currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
                break;
            case "hunger":
                currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
                break;
            case "thirst":
                currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst);
                break;
            case "sleep":
                currentSleep = Mathf.Clamp(currentSleep + amount, 0, maxSleep);
                break;
        }
        onStatsChanged?.Invoke();
    }

    // A special boolean method for stamina. It returns TRUE if you had enough energy, and FALSE if you are too tired.
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            onStatsChanged?.Invoke();
            return true; 
        }
        
        Debug.Log("Not enough stamina!");
        return false; 
    }
}