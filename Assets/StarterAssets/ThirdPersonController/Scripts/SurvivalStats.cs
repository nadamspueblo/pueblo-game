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
  public float staminaRegenDelay = 1.0f; // Wait 1 second after running before regening
  public float exhaustionDelay = 3.0f;   // Wait 3 seconds if the bar hits absolute zero!
  public bool isExhausted = false;
  public bool isDead = false;

  [Header("Component References")]
  public AttackController attackController;

  [Header("Sound Effects")]
  public AudioSource audioSource;
  public AudioClip wakeUpSound;
  public AudioClip outOfBreath;
  public float audioCoolDown = 5f;
  private float audioCoolDownTimer = 0f;

  [Header("Events")]
  // We will use these later to tell the UI Canvas to update its bars
  public UnityEvent onStatsChanged;
  public UnityEvent onPlayerDeath;
  public UnityEvent<float, Transform> onTakeDamage;

  private float nextStaminaRegenTime = 0f;

  void Start()
  {
    // 1. Initialize all stats to their maximums when the game begins
    currentHealth = maxHealth;
    currentHunger = maxHunger;
    currentThirst = maxThirst;
    currentSleep = maxSleep;
    currentStamina = maxStamina;

    if (attackController == null) attackController = GetComponent<AttackController>();
  }

  void Update()
  {
    if (isDead) return;
    HandlePassiveDrain();
    HandleStaminaRegen();

    if (currentStamina / maxStamina < 0.38f && !audioSource.isPlaying)
    {
      PlayAudio(outOfBreath);
    }
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
      TakeDamage(2f * Time.deltaTime, null);
    }

    // Fire off a message that stats have changed for UI update
    onStatsChanged?.Invoke();
  }

  private void HandleStaminaRegen()
  {
    // Stamina naturally regenerates over time if it isn't full
    if (currentStamina < maxStamina && Time.time >= nextStaminaRegenTime)
    {
      currentStamina += staminaRegenRate * (currentHunger + currentSleep + currentThirst) / (maxHunger + maxSleep + maxThirst) * (currentHealth / maxHealth) * Time.deltaTime;
      if (isExhausted && currentStamina >= 15f) // Adjust this 15f threshold however you like!
      {
        isExhausted = false;
        Debug.Log("Recovered from exhaustion!");
      }
      //if (currentStamina / maxStamina >= 0.5f) StopAudio();

      currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
      onStatsChanged?.Invoke();
    }
  }

  private void PlayAudio(AudioClip clip)
  {
    if (clip != null && audioSource != null) //
    {
      //audioSource.pitch = Random.Range(0.8f, 1.2f); //
      audioSource.PlayOneShot(clip); //
    }
  }

  private void StopAudio()
  {
    if (audioSource != null && audioSource.isPlaying)
    {
      audioSource.Stop();
    }
  }
  // --- PUBLIC METHODS FOR OTHER SCRIPTS TO USE ---

  public void TakeDamage(float amount, Transform attackerTransform)
  {
    if (isDead) return;
    currentHealth -= attackController.isBlocking && UseStamina(attackController.blockStamina) ? 0.5f * amount : amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    if (currentHealth <= 0)
    {
      Debug.Log("Player has died!");
      isDead = true;
      onPlayerDeath?.Invoke();
    }
    else
    {
      onTakeDamage?.Invoke(amount, attackerTransform);
    }
    onStatsChanged?.Invoke();
  }

  // Use this when eating food, drinking water, or sleeping
  public void RestoreStat(string statName, float amount)
  {
    if (isDead) return;
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
    if (isExhausted || isDead) return false;

    if (currentStamina >= amount)
    {
      currentStamina -= amount;

      // Push the regen timer into the future by the standard delay
      nextStaminaRegenTime = Time.time + staminaRegenDelay;

      // EXHAUSTION PENALTY: Did we just completely empty the tank?
      if (currentStamina <= 0.05f) // Using 0.05f to catch tiny float decimals
      {
        currentStamina = 0f;
        isExhausted = true;
        nextStaminaRegenTime = Time.time + exhaustionDelay;
        Debug.Log($"Player is exhausted! {nextStaminaRegenTime}");
      }

      onStatsChanged?.Invoke();
      return true;
    }

    // If they try to sprint while exhausted, keep resetting the penalty timer!
    nextStaminaRegenTime = Time.time + staminaRegenDelay;
    return false;
  }

  public void PlayWakeUpSound()
  {
    PlayAudio(wakeUpSound);
  }
}