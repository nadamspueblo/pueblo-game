using System.Collections;
using UnityEngine;

public class CombatEffects : MonoBehaviour
{
    // Call this exactly when the axe's hitbox hits the zombie!
    public void HitStop(float duration = 0.05f) 
    {
        // 1. Freeze the game (or slow it down to 10% speed)
        Time.timeScale = 0.1f; 
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("HitStop Called");
        StartCoroutine(HitPauseRoutine(duration));
    }

    private IEnumerator HitPauseRoutine(float duration)
    {
      Debug.Log("HitPauseRoutine started");
        
        // 2. Wait for a fraction of a second. 
        // We MUST use Realtime, otherwise the paused game time will make this wait forever!
        yield return new WaitForSecondsRealtime(duration);
        
        // 3. Unfreeze the game
        Time.timeScale = 1f; 
        Time.fixedDeltaTime = 0.02f;
        Debug.Log("HitPauseRoutine ended");
    }
}