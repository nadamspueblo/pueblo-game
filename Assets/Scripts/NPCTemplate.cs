using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

[RequireComponent(typeof(NavMeshAgent))]
public class NPCTemplate : MonoBehaviour
{
    [Header("AI Settings")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    
    [Header("Chasing Settings")]
    public Transform targetToChase; // Drag the Player here in the Inspector
    public float chaseDistance = 15f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;

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
        if (distanceToTarget <= chaseDistance)
        {
            ChaseTarget();
        }
        else
        {
            WanderAround();
        }

        // 3. Update the Animator
        // NavMeshAgent.velocity handles the speed math for us!
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            
            // If you are using the 2D Blend Tree (VelocityX/Z), we just send the forward speed
            // If you are using a 1D Blend Tree, change "VelocityZ" to "Speed"
            animator.SetFloat("VelocityZ", speed, 0.1f, Time.deltaTime);
            animator.SetFloat("VelocityX", 0f, 0.1f, Time.deltaTime); // NPCs usually don't strafe
        }
    }

    private void ChaseTarget()
    {
        // Tell the AI to move to the target's exact position
        agent.SetDestination(targetToChase.position);
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