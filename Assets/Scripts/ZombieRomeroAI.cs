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
  public float walkSpeed = 1f;
  public float chaseDistance = 15f;
  public float chaseSpeed = 2f;
  public float rushDistance = 5f;
  public float rushSpeed = 4f;
  public float attackDistance = 1.5f;
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

  private HealthManager healthManager;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponentInChildren<Animator>();
    timer = wanderTimer;
    healthManager = GetComponent<HealthManager>();

    if (targetToChase == null)
    {
      targetToChase = GameObject.FindWithTag("Player").transform;
    }
  }

  void Update()
  {
    if (healthManager.IsDead()) return;
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
      speed = 0f;
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
    else if (distanceToTarget <= chaseDistance)
    {
      float trueWalkingDistance = GetActualPathDistance(targetToChase.position);
      float distToObstacle = GetDistanceToClosetObstacle(targetToChase.position);

      if (trueWalkingDistance <= rushDistance)
      {
        if (!hasRushed)
        {
          PlayRushSound();
          hasRushed = true;
        }
        RushTarget();
      }
      else if (trueWalkingDistance <= chaseDistance)
      {
        if (!hasAlerted)
        {
          PlayAlertSound();
          hasAlerted = true;
        }
        hasRushed = false;
        ChaseTarget();
      }
      else if (distToObstacle > 0.5f && distToObstacle < chaseDistance)
      {
        hasRushed = false;
        ChaseTarget();
        HandleIdleGrowls();
      }
      else if (distanceToTarget < rushDistance)
      {
        if (!hasRushed)
        {
          PlayRushSound();
          hasRushed = true;
        }
        FaceTarget();
        if (Time.time >= lastAttackTime + attackCooldown + Random.Range(2f, 5f))
        {
            PlayAttackSound();
            AttackTarget();
            speed = 0f;
            animator.SetFloat("VelocityZ", speed, 0.1f, Time.deltaTime);
        }
      }
      else
      {
        agent.ResetPath(); // Delete the GPS route so it stops walking!
        speed = 0f;

        HandleIdleGrowls();
        FaceTarget();
        animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);

        hasRushed = false;
        hasAlerted = false;
      }
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
    speed = 0f;
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
    speed = chaseSpeed;
    agent.SetDestination(targetToChase.position);
  }

  private void RushTarget()
  {
    agent.SetDestination(targetToChase.position);
    speed = rushSpeed;
  }

  private void WanderAround()
  {
    timer += Time.deltaTime;
    speed = walkSpeed;

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

  private float GetActualPathDistance(Vector3 targetPosition)
  {
    NavMeshPath path = new NavMeshPath();

    // Ask the NavMesh to draw a path from the zombie to the target
    if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
    {
      if (path.status != NavMeshPathStatus.PathComplete)
      {
        return Mathf.Infinity; // The player is unreachable!
      }

      float distance = 0f;

      // Measure the length of every line segment in the path
      for (int i = 1; i < path.corners.Length; i++)
      {
        distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
      }
      return distance;
    }

    // If there is no valid path (e.g. player is on a roof), return infinity
    return Mathf.Infinity;
  }

  private float GetDistanceToClosetObstacle(Vector3 targetPosition)
  {
    NavMeshPath path = new NavMeshPath();
    if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
    {
      float distance = 0f;

      // Measure the length of every line segment in the path
      for (int i = 1; i < path.corners.Length; i++)
      {
        distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
      }
      return distance;
    }
    return Mathf.Infinity;
  }
}