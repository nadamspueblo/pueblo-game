using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

[RequireComponent(typeof(NavMeshAgent))]
public class CopZombieAI : MonoBehaviour
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

  private float speed = 2f;
  private NavMeshAgent agent;
  private Animator animator;
  private float timer;
  private float lastAttackTime;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponentInChildren<Animator>();
    timer = wanderTimer;
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
      RushTarget();
    }
    else if (distanceToTarget <= chaseDistance)
    {
      ChaseTarget();
    }
    else
    {
      WanderAround();
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
    speed = 2f;
    agent.SetDestination(targetToChase.position);
  }

  private void RushTarget()
  {
    agent.SetDestination(targetToChase.position);
    speed = 4f;
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