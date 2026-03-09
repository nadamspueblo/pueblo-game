using UnityEngine;
using UnityEngine.UI; // We must include this to talk to UI Images!

public class SurvivalUIManager : MonoBehaviour
{
  [Header("Player Data")]
  public SurvivalStats playerStats; // Drag the Player here

  [Header("UI Fill Bars")]
  public Image healthBarFill;
  public Image hungerBarFill;
  public Image thirstBarFill;
  public Image sleepBarFill;
  public Image staminaBarFill;

  void Start()
  {
    // Force the UI to update the exact moment the game starts
    UpdateAllBars();
  }

  // This is the public function we will trigger using the Unity Event
  public void UpdateAllBars()
  {
    // Safety check so the game doesn't crash if the player is missing
    if (playerStats == null) return;

    // Calculate the percentages (0.0 to 1.0)
    float healthPct = playerStats.currentHealth / playerStats.maxHealth;
    float hungerPct = playerStats.currentHunger / playerStats.maxHunger;
    float thirstPct = playerStats.currentThirst / playerStats.maxThirst;
    float sleepPct = playerStats.currentSleep / playerStats.maxSleep;
    float staminaPct = playerStats.currentStamina / playerStats.maxStamina;

    // Update the physical fill amounts
    if (healthBarFill != null) healthBarFill.fillAmount = healthPct;
    if (hungerBarFill != null) hungerBarFill.fillAmount = hungerPct;
    if (thirstBarFill != null) thirstBarFill.fillAmount = thirstPct;
    if (sleepBarFill != null) sleepBarFill.fillAmount = sleepPct;
    if (staminaBarFill != null) staminaBarFill.fillAmount = staminaPct;

    // Update the colors based on those percentages!
    UpdateBarColor(healthBarFill, healthPct);
    UpdateBarColor(hungerBarFill, hungerPct);
    UpdateBarColor(thirstBarFill, thirstPct);
    UpdateBarColor(sleepBarFill, sleepPct);
    UpdateBarColor(staminaBarFill, staminaPct);
  }

  private void UpdateBarColor(Image bar, float percentage)
  {
    if (bar == null) return;

    if (percentage > 0.5f)
    {
      bar.color = Color.white; // Good!
    }
    else if (percentage > 0.25f)
    {
      bar.color = Color.yellow; // Warning!
    }
    else
    {
      bar.color = Color.red; // Danger!
    }
  }
}