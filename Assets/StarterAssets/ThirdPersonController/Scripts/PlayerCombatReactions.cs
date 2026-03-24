using System.Collections;
using StarterAssets;
using UnityEngine;

public class PlayerCombatReactions : MonoBehaviour
{
  [Header("References")]
  public ThirdPersonController thirdPersonController;
  public Animator playerAnimator;
  public AttackController attackController;
  public SurvivalStats stats;
  private ZombieAdvancedAI attackingZombie;
  public bool isGrappled = false;

  void Start()
  {
    // Tune the radio! When SurvivalStats broadcasts a hit, run our ReactToHit function.
    if (stats != null)
    {
      stats.onTakeDamage.AddListener(ReactToHit);
    }
    if (thirdPersonController == null) thirdPersonController = GetComponent<ThirdPersonController>();
    thirdPersonController.onGrappleBreak.AddListener(BreakGrapple);
  }

  public void ReactToGrapple(float duration, ZombieAdvancedAI zombie)
  {
    if (playerAnimator == null || zombie == null) return;
    if (attackController.isBlocking && stats.UseStamina(attackController.blockStamina))
    {
      playerAnimator.SetTrigger("BreakGrab");
      zombie.BreakGrapple();
    }
    else
    {
      //thirdPersonController.SetGrappleState(true, zombie);
      attackingZombie = zombie;
      thirdPersonController.StartGrapple(0.7f);
      playerAnimator.SetBool("IsGrappled", true);
      isGrappled = true;
      StartCoroutine(HitPauseRoutine(duration));
    }
  }

  private void BreakGrapple()
  {
    attackingZombie.BreakGrapple();
    playerAnimator.SetBool("IsGrappled", false);
    isGrappled = false;
  }

  public void ReactToBite()
  {

  }

  public void EndBiteGrapple()
  {
    playerAnimator.SetBool("IsGrappled", false);
    isGrappled = false;
    thirdPersonController.SetState(PlayerMovementState.FreeExplore);
  }

  // This function automatically runs whenever the event fires
  public void ReactToHit(float damage, Transform attacker)
  {
    if (playerAnimator == null || attacker == null) return;

    // 1. Calculate the normalized direction vector to the attacker
    Vector3 dirToAttacker = (attacker.position - transform.position).normalized;

    // 2. Calculate the Dot Products for our 2D Blend Tree
    float hitZ = Vector3.Dot(transform.forward, dirToAttacker); // Front/Back
    float hitX = Vector3.Dot(transform.right, dirToAttacker);   // Right/Left

    // 3. Are we blocking?
    if (attackController != null && attackController.isBlocking)
    {
      // Check if the attack is actually coming from the front!
      if (hitZ > 0.5f)
      {
        playerAnimator.SetTrigger("Hit"); // Triggers the Upper Body block flinch
        stats.UseStamina(attackController.blockStamina);
        return; // Stop here so we don't play the full body stagger!
      }
    }

    // 4. We got hit! Send the math to the Animator for the Full Body override
    playerAnimator.SetFloat("HitX", hitX);
    playerAnimator.SetFloat("HitZ", hitZ);
    playerAnimator.SetTrigger("Hit");
  }

  private IEnumerator HitPauseRoutine(float duration)
  {
    Time.timeScale = 0.1f;
    Time.fixedDeltaTime = 0.02f * Time.timeScale;
    yield return new WaitForSecondsRealtime(duration);
    Time.timeScale = 1f;
    Time.fixedDeltaTime = 0.02f;
  }

  private IEnumerator BiteGrappleRoutine(float duration)
  {
    float timer = 0f;
    isGrappled = true;

    // Slow time
    Time.timeScale = 0.1f;
    Time.fixedDeltaTime = 0.02f * Time.timeScale;

    while (isGrappled && timer < duration)
    {
      timer += Time.deltaTime;
      yield return null;
    }

    // Return to normal time
    Time.timeScale = 1f;
    Time.fixedDeltaTime = 0.02f;
  }
}