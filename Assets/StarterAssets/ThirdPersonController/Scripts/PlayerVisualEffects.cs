using UnityEngine;
using UnityEngine.Rendering;

public class PlayerVisualEffects : MonoBehaviour
{
  [Header("Core References")]
  [Tooltip("Reference to the player's stats script")]
  public SurvivalStats survivalStats;

  [Tooltip("Drag your HealthEffects Global Volume here")]
  public Volume healthVolume;

  [Tooltip("Drag your StaminaEffects Global Volume here")]
  public Volume staminaVolume;

  [Header("Effect Thresholds")]
  [Tooltip("Start fading to black when health drops below this percentage (e.g., 0.3 = 30%)")]
  [Range(0f, 1f)] public float healthEffectStart = 0.51f;

  [Tooltip("Start blurring/whitening when stamina drops below this percentage (e.g., 0.2 = 20%)")]
  [Range(0f, 1f)] public float staminaEffectStart = 1f;

  public float wakeTime = 3f;
  private float wakeTimer = 0f;
  private bool isAwake = false;

  void Update()
  {
    // Safety check to prevent errors if the stats script isn't linked
    if (survivalStats == null) return;

    UpdateHealthEffects();
    UpdateStaminaEffects();
    if (!isAwake) UpdateAwakeEffects();
  }

  private void UpdateHealthEffects()
  {
    if (healthVolume == null) return;

    // 1. Calculate our current health percentage (0.0 to 1.0)
    float healthPercent = survivalStats.currentHealth / survivalStats.maxHealth;

    // 2. InverseLerp maps our threshold to a clean 0 to 1 scale.
    // If health is AT or ABOVE the start threshold, weight is 0.
    // As health drops toward 0, weight smoothly climbs to 1.
    float targetWeight = Mathf.InverseLerp(healthEffectStart, 0f, healthPercent);

    // 3. Apply the calculated weight directly to the Volume
    healthVolume.weight = targetWeight;
  }

  private void UpdateAwakeEffects()
  {
    float progress = wakeTimer / wakeTime;
    wakeTimer += Time.deltaTime;
    float targetWeight = Mathf.Lerp(1f, 0f, progress);

    // 3. Apply the calculated weight directly to the Volume
    healthVolume.weight = targetWeight;
    
    if (progress >= 1f) isAwake = true;
  }

  private void UpdateStaminaEffects()
  {
    if (staminaVolume == null) return;

    // 1. Calculate our current stamina percentage (0.0 to 1.0)
    float staminaPercent = survivalStats.currentStamina / survivalStats.maxStamina;

    // 2. Map the threshold to a 0 to 1 scale
    float targetWeight = Mathf.InverseLerp(staminaEffectStart, 0f, staminaPercent);

    // 3. Apply to the Volume
    staminaVolume.weight = targetWeight;
  }
}