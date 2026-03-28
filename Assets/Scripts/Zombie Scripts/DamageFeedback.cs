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
  public RagdollManager ragdollManager;
  private ZombieAdvancedAI zombieAdvancedAI;

  void Start()
  {
    if (anim == null) anim = GetComponentInChildren<Animator>();
    if (audioSource == null) audioSource = GetComponent<AudioSource>();
    if (healthManager == null) healthManager = GetComponent<HealthManager>();
    if (healthManager != null) { healthManager.onDeath.AddListener(PlayDeathReaction); }
    zombieAdvancedAI = GetComponent<ZombieAdvancedAI>();

    // Note: If you still have generic damage sources (like fire/poison), 
    // you can keep an event listener here that points to a generic hit reaction!

    agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
  }

  // Your ZombieBodyPart colliders will call this directly when struck by the axe!
  public void PlayLocationalReaction(ZombieBodyPart.PartType partHit, Vector3 hitPoint, Transform attacker, Transform hitBone, float damage)
  {
    // 1. Audio
    if (audioSource != null && takeDamageSound != null)
    {
      audioSource.PlayOneShot(takeDamageSound);
    }

    // 2. Spawn Blood at the exact point of impact
    Vector3 dirToAttacker = (attacker.position - transform.position).normalized;
    if (bloodSplatterPrefab != null)
    {
      GameObject bloodVFX = Instantiate(bloodSplatterPrefab, hitPoint, Quaternion.LookRotation(dirToAttacker), hitBone);
      Destroy(bloodVFX, 10.0f);
    }

    // 3. Snap Rotation
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
        
        anim.SetTrigger("Hit");
        anim.SetFloat("DamageAmount", damage);
        anim.SetFloat("HitX", dirToAttacker.x);
        if (damage >= 50)
        {
          if (attacker.CompareTag("Player")) TriggerHeadshot();
          zombieAdvancedAI.Event_ChangeState("Unconscious");
        }
        break;
      case ZombieBodyPart.PartType.Arm:
      case ZombieBodyPart.PartType.Torso:
        anim.SetTrigger("Hit"); // Your default stumble
        anim.SetFloat("DamageAmount", damage);
        anim.SetFloat("HitX", dirToAttacker.x);
        break;
      case ZombieBodyPart.PartType.Leg:
        anim.SetTrigger("HitLegs"); 
        break;
    }
  }

  private void TriggerHeadshot()
  {
    // Start the AAA Hit Pause (Slow time to 10% for 0.1 seconds)
    StartCoroutine(HitPauseRoutine(0.5f));

    // Twist the head bone for half a second
    //headTwistTimer = 0.5f;
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
    }

    if (audioSource != null && deathSound != null)
    {
      audioSource.PlayOneShot(deathSound);
    }

    zombieAdvancedAI.Event_ChangeState("Dead");

    // Disable AI completely
    if (agent != null && agent.isOnNavMesh)
    {
      agent.isStopped = true;
      agent.enabled = false;
    }

    // Enable ragdoll
    if (ragdollManager != null)
    {
      ragdollManager.EnableRagdoll();
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