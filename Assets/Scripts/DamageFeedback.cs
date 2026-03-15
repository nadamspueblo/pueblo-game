using UnityEngine;
using System.Collections; // Required for the HitPause Coroutine!

public class DamageFeedback : MonoBehaviour
{
  [Header("Animations")]
  public Animator anim;
  public UnityEngine.AI.NavMeshAgent agent;

  [Header("Audio")]
  public AudioSource audioSource;
  public AudioClip takeDamageSound;
  public AudioClip deathSound;
  public HealthManager healthManager;

  [Header("Locational VFX & Reactions")]
  public GameObject bloodSplatterPrefab;
  public Transform headBone;
  private float headTwistTimer = 0f;

  void Start()
  {
    if (anim == null) anim = GetComponentInChildren<Animator>();
    if (audioSource == null) audioSource = GetComponent<AudioSource>();
    if (healthManager == null) healthManager = GetComponent<HealthManager>();
    if (healthManager != null) { healthManager.onDeath.AddListener(PlayDeathReaction); }

    // Note: If you still have generic damage sources (like fire/poison), 
    // you can keep an event listener here that points to a generic hit reaction!

    agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
  }

  // Your ZombieBodyPart colliders will call this directly when struck by the axe!
  public void PlayLocationalReaction(ZombieBodyPart.PartType partHit, Vector3 hitPoint, Transform attacker, Transform hitBone)
  {
    // 1. Audio
    if (audioSource != null && takeDamageSound != null)
    {
      audioSource.PlayOneShot(takeDamageSound);
    }

    // 2. Spawn Blood at the exact point of impact
    if (bloodSplatterPrefab != null)
    {
      Vector3 directionToAttacker = (attacker.position - hitPoint).normalized;
      GameObject bloodVFX = Instantiate(bloodSplatterPrefab, hitPoint, Quaternion.LookRotation(directionToAttacker), hitBone);
      Destroy(bloodVFX, 10.0f);
    }

    // 3. Snap Rotation
    Vector3 dirToAttacker = (attacker.position - transform.position).normalized;
    dirToAttacker.y = 0;

    if (dirToAttacker != Vector3.zero)
    {
      transform.rotation = Quaternion.LookRotation(dirToAttacker);
    }

    // Pause NavMeshAgent
    if (agent != null && agent.isOnNavMesh)
    {
      agent.isStopped = true;
      agent.updateRotation = false;
      Invoke("ResumeAgent", 1.0f);
    }

    // 4. Locational Reactions
    switch (partHit)
    {
      case ZombieBodyPart.PartType.Head:
        TriggerHeadshot();
        break;
      case ZombieBodyPart.PartType.Torso:
        anim.SetTrigger("Hit"); // Your default stumble
        break;
      case ZombieBodyPart.PartType.Legs:
        anim.SetTrigger("HitLegs"); // Make sure this trigger exists in the Animator!
        break;
    }
  }

  private void TriggerHeadshot()
  {
    // Start the AAA Hit Pause (Slow time to 10% for 0.1 seconds)
    StartCoroutine(HitPauseRoutine(0.5f));

    // Twist the head bone for half a second
    headTwistTimer = 0.5f;

    // Still trigger the torso stumble so the body reacts!
    anim.SetTrigger("Hit");
  }

  void LateUpdate()
  {
    // Physically crank the neck bone sideways if a headshot just happened
    if (headTwistTimer > 0)
    {
      headTwistTimer -= Time.deltaTime;
      if (headBone != null)
      {
        // Adjust these axes (X, Y, or Z) based on how your specific zombie rig is oriented!
        headBone.Rotate(new Vector3(60f, 0, 0), Space.Self);
      }
    }
  }

  private IEnumerator HitPauseRoutine(float duration)
  {
    Time.timeScale = 0.1f;
    yield return new WaitForSecondsRealtime(duration);
    Time.timeScale = 1f;
  }

  // Linked to the HealthManager's OnDeath event
  public void PlayDeathReaction()
  {
    if (anim != null)
    {
      // Force the Animator to forget any pending flinch commands!
      anim.ResetTrigger("Hit");
      anim.ResetTrigger("HitLegs");
      anim.SetTrigger("Death");
    }

    if (audioSource != null && deathSound != null)
    {
      audioSource.PlayOneShot(deathSound);
    }

    // Disable AI completely
    if (agent != null && agent.isOnNavMesh)
    {
      agent.isStopped = true;
    }

    // NEW: Disable every collider on the zombie so the axe passes through the corpse
    Collider[] allColliders = GetComponentsInChildren<Collider>();
    foreach (Collider col in allColliders)
    {
      col.enabled = false;
    }
  }

  void ResumeAgent()
  {
    if (agent != null && agent.isOnNavMesh)
    {
      agent.isStopped = false;
      agent.updateRotation = true;
    }
  }
}