using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI controller for a companion dog that follows the player and reacts to the environment.
/// Uses NavMeshAgent for pathfinding and Animator for animation control.
/// </summary>
public class DogAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player; // Drag the player GameObject here in Inspector
    [SerializeField] private Animator animator; // Reference to the dog's Animator component
    [SerializeField] private NavMeshAgent agent; // Reference to the NavMeshAgent component

    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 3f; // Dog stops moving when within this distance
    [SerializeField] private float idleDistance = 2f; // Dog enters idle state when this close
    [SerializeField] private float runThreshold = 6f; // Dog runs if farther than this distance

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f; // How far the dog wanders when idle
    [SerializeField] private float wanderTimer = 5f; // Time between random wander movements
    private float wanderCountdown;

    [Header("Animation Parameters")]
    private readonly int speedHash = Animator.StringToHash("Speed"); // Float parameter for blend tree
    private readonly int isIdlePlayingHash = Animator.StringToHash("IsIdlePlaying"); // Bool for playing animation

    // State tracking
    private enum DogState { Idle, Following, Running }
    private DogState currentState = DogState.Idle;

    void Start()
    {
        // Auto-find references if not assigned
        if (animator == null) animator = GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else Debug.LogError("DogAI: No player assigned and couldn't find GameObject with 'Player' tag!");
        }

        wanderCountdown = wanderTimer;
    }

    void Update()
    {
        if (player == null) return; // Safety check

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determine state based on distance
        if (distanceToPlayer > runThreshold)
        {
            currentState = DogState.Running;
        }
        else if (distanceToPlayer > followDistance)
        {
            currentState = DogState.Following;
        }
        else
        {
            currentState = DogState.Idle;
        }

        // Execute behavior based on current state
        switch (currentState)
        {
            case DogState.Idle:
                HandleIdle();
                break;
            case DogState.Following:
                HandleFollowing();
                break;
            case DogState.Running:
                HandleRunning();
                break;
        }

        // Update animator with agent's current speed
        UpdateAnimator();
    }

    /// <summary>
    /// Idle behavior: Dog stays near player, occasionally wanders or plays
    /// </summary>
    private void HandleIdle()
    {
        // Stop moving
        agent.isStopped = true;

        // Random idle playing animation trigger
        if (Random.value < 0.01f) // 1% chance per frame (~once every few seconds)
        {
            animator.SetBool(isIdlePlayingHash, true);
        }
        else
        {
            animator.SetBool(isIdlePlayingHash, false);
        }

        // Optional: Random wander near player
        wanderCountdown -= Time.deltaTime;
        if (wanderCountdown <= 0f)
        {
            WanderNearPlayer();
            wanderCountdown = wanderTimer;
        }
    }

    /// <summary>
    /// Following behavior: Dog walks toward player at normal speed
    /// </summary>
    private void HandleFollowing()
    {
        agent.isStopped = false;
        agent.speed = 2f; // Walking speed
        agent.SetDestination(player.position);
        animator.SetBool(isIdlePlayingHash, false);
    }

    /// <summary>
    /// Running behavior: Dog runs quickly to catch up to player
    /// </summary>
    private void HandleRunning()
    {
        agent.isStopped = false;
        agent.speed = 5f; // Running speed
        agent.SetDestination(player.position);
        animator.SetBool(isIdlePlayingHash, false);
    }

    /// <summary>
    /// Makes the dog wander to a random point near the player
    /// </summary>
    private void WanderNearPlayer()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += player.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.speed = 1.5f;
            agent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// Updates the Animator with the dog's current movement speed
    /// </summary>
    private void UpdateAnimator()
    {
        // Normalize agent velocity to 0-1 range for blend tree
        // 0 = idle, 0.5 = walk, 1.0 = run
        float normalizedSpeed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(speedHash, normalizedSpeed);
    }

    // Visualize follow distances in Scene view
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, idleDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, runThreshold);
    }
}
