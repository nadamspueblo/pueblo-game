using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieRomeroAI : MonoBehaviour
{
  [Header("AI Settings")]
  public float wanderRadius = 10f;
  public float wanderTimer = 5f;

  [Header("Chasing & Combat")]
  public Transform targetToChase; // Drag the Player here in the Inspector
  public float chaseDistance = 15f;
  public float rushDistance = 5f;
  public float attackDistance = 2f;
  public float attackCooldown = 2f;

  private float speed = 1f;
  private NavMeshAgent agent;
  private Animator animator;
  private float timer;
  private float lastAttackTime;

  [Header("Audio Setup")]
  public AudioSource audioSource;
  public AudioClip alertSound;
  public AudioClip rushSound;
  public AudioClip attachSound;
  public AudioClip[] idleGrowls;

  private bool hasAlerted = false;
  private bool hasRushed = false;

  // Timer for random idle growls
  private float growlTimer = 0f;
  private float nextGrowlTime = 5f;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponentInChildren<Animator>();
    timer = wanderTimer;

    if (targetToChase == null)
    {
      targetToChase = GameObject.FindWithTag("Player").transform;
    }
  }

  void Update()
  {
    // 1. Calculate the distance to the target (Player)
    float distanceToTarget = Mathf.Infinity;
    if (targetToChase != null)
    {
      distanceToTarget = Vector3.Distance(transform.position, targetToChase.position);
    }

    // 2. Decide what to do based on distance
    if (distanceToTarget <= attackDistance)
    {
      // We are close enough! But is our cooldown finished?
      if (Time.time >= lastAttackTime + attackCooldown)
      {
        PlayAttackSound();
        AttackTarget();
      }
      else
      {
        // We are close, but waiting to attack again. Stand still and stare at the player.
        FaceTarget();
        animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
      }
    }
    else if (distanceToTarget <= rushDistance)
    {
      if (!hasRushed)
      {
        PlayRushSound();
        hasRushed = true;
      }
      RushTarget();
    }
    else if (distanceToTarget <= chaseDistance)
    {
      if (!hasAlerted)
      {
          PlayAlertSound();
          hasAlerted = true; 
      }
      hasRushed = false;
      ChaseTarget();
    }
    else
    {
      hasAlerted = false;
      WanderAround();
      HandleIdleGrowls();
    }

    // 3. Update the Animator
    if (animator != null && !agent.isStopped)
    {
      // If you are using a 1D Blend Tree, change "VelocityZ" to "Speed"
      animator.SetFloat("VelocityZ", speed, 0.1f, Time.deltaTime);
    }
  }

  private void AttackTarget()
  {
    // Look at the player so the zombie doesn't attack thin air
    FaceTarget();

    // Trigger the animation
    if (animator != null)
    {
      animator.SetTrigger("Attack");
    }
    lastAttackTime = Time.time;
  }

  private void FaceTarget()
  {
    Vector3 direction = (targetToChase.position - transform.position).normalized;
    direction.y = 0; // Keep it flat so the zombie doesn't tilt upward
    Quaternion lookRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
  }

  private void ChaseTarget()
  {
    // Tell the AI to move to the target's exact position
    speed = 1f;
    agent.SetDestination(targetToChase.position);
  }

  private void RushTarget()
  {
    agent.SetDestination(targetToChase.position);
    speed = 3.5f;
  }

  private void WanderAround()
  {
    timer += Time.deltaTime;

    // If it's time to pick a new random spot...
    if (timer >= wanderTimer)
    {
      Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
      agent.SetDestination(newPos);
      timer = 0;
    }
  }

  // Audio functions
  private void PlayAlertSound()
  {
    if (alertSound != null && audioSource != null)
    {
      // Randomize pitch for variation
      audioSource.pitch = Random.Range(0.8f, 1.2f);
      // PlayOneShot allows multiple sounds to overlap without cutting each other off
      audioSource.PlayOneShot(alertSound);
    }
  }

  private void PlayRushSound()
  {
    if (rushSound != null && audioSource != null)
    {
      // Randomize pitch for variation
      audioSource.pitch = Random.Range(0.8f, 1.2f);
      // PlayOneShot allows multiple sounds to overlap without cutting each other off
      audioSource.PlayOneShot(rushSound);
    }
  }

  private void PlayAttackSound()
  {
    if (attachSound != null && audioSource != null)
    {
      // Randomize pitch for variation
      audioSource.pitch = Random.Range(0.8f, 1.2f);
      // PlayOneShot allows multiple sounds to overlap without cutting each other off
      audioSource.PlayOneShot(attachSound);
    }
  }

  private void HandleIdleGrowls()
  {
    growlTimer += Time.deltaTime;
    if (growlTimer >= nextGrowlTime)
    {
      if (idleGrowls.Length > 0)
      {
        // Pick a random growl from the list
        int randomIndex = Random.Range(0, idleGrowls.Length);
        // Randomize pitch for variation
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(idleGrowls[randomIndex]);
      }
      SetNextGrowlTime();
    }
  }

  private void SetNextGrowlTime()
  {
    growlTimer = 0f;
    // Randomize how often it growls (e.g., between 4 and 10 seconds)
    nextGrowlTime = Random.Range(4f, 10f);
  }

  // A helper math function to find a random walkable point on the NavMesh
  public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
  {
    Vector3 randDirection = Random.insideUnitSphere * dist;
    randDirection += origin;

    NavMeshHit navHit;
    // Check if the random point is actually on the walkable floor
    NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

    return navHit.position;
  }
}