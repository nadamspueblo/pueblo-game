using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    
    [Header("Discovery Settings")]
    [SerializeField] private float discoveryRange = 5f;
    private bool isDiscovered = false;
    
    [Header("Following Settings")]
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderInterval = 5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private LayerMask zombieLayer;
    
    private enum DogState { Idle, Following, Wandering, Attacking }
    private DogState currentState = DogState.Idle;
    
    private float wanderTimer;
    private float attackTimer;
    private Transform currentTarget;
    
    void Start()
    {
        // Auto-find the NavMeshAgent if not assigned
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        // Auto-find the Animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Auto-find the player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("DogAI: Cannot find player! Make sure player has 'Player' tag.");
            }
        }
        
        wanderTimer = wanderInterval;
        
        Debug.Log("DogAI: Initialized. Waiting to be discovered...");
    }
    
    void Update()
    {
        // Check if player has discovered the dog yet
        if (!isDiscovered)
        {
            CheckForDiscovery();
            return;
        }
        
        // Decrement attack timer
        attackTimer -= Time.deltaTime;
        
        // Run state machine
        switch (currentState)
        {
            case DogState.Idle:
                IdleState();
                break;
            case DogState.Following:
                FollowingState();
                break;
            case DogState.Wandering:
                WanderingState();
                break;
            case DogState.Attacking:
                AttackingState();
                break;
        }
        
        // Always check for zombies threatening the player
        CheckForThreats();
    }
    
    // ========== DISCOVERY ==========
    void CheckForDiscovery()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= discoveryRange)
        {
            isDiscovered = true;
            currentState = DogState.Following;
            Debug.Log("Dog discovered by player! Now following.");
        }
    }
    
    // ========== STATE: IDLE ==========
    void IdleState()
    {
        // Just sit still until discovered
        agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
    
    // ========== STATE: FOLLOWING ==========
    void FollowingState()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // If too far from player, move closer
        if (distanceToPlayer > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            
            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
        }
        else
        {
            // Close enough, maybe wander nearby
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                currentState = DogState.Wandering;
                wanderTimer = wanderInterval;
            }
            else
            {
                agent.isStopped = true;
                
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
            }
        }
    }
    
    // ========== STATE: WANDERING ==========
    void WanderingState()
    {
        if (player == null) return;
        
        // Pick a random point near the player
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += player.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
        
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        
        // After some time, go back to following
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            currentState = DogState.Following;
            wanderTimer = wanderInterval;
        }
    }
    
    // ========== STATE: ATTACKING ==========
    void AttackingState()
    {
        // If target is dead, gone, or doesn't have health, stop attacking
        if (currentTarget == null || IsTargetDead())
        {
            currentTarget = null;
            currentState = DogState.Following;
            Debug.Log("Dog: Target lost or dead, returning to player.");
            return;
        }
        
        // CHECK: If player is too far away, abandon the fight and follow player
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > followDistance * 3f) // If player is 3x the normal follow distance
            {
                currentTarget = null;
                currentState = DogState.Following;
                Debug.Log("Dog: Player too far! Abandoning fight to catch up.");
                return;
            }
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        
        // Move toward the zombie
        if (distanceToTarget > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
            
            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
        }
        else
        {
            // Close enough to bite
            agent.isStopped = true;
            
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            
            // Face the zombie
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            
            // Attack if cooldown is ready
            if (attackTimer <= 0f)
            {
                PerformBiteAttack();
                attackTimer = attackCooldown;
            }
        }
        
        // If zombie gets too far, give up
        if (distanceToTarget > detectionRange * 1.5f)
        {
            currentTarget = null;
            currentState = DogState.Following;
            Debug.Log("Dog: Zombie too far, returning to player.");
        }
    }
    
    // ========== CHECK IF TARGET IS DEAD ==========
    bool IsTargetDead()
    {
        if (currentTarget == null) return true;
        
        // Try to get the HealthManager component from the zombie
        HealthManager targetHealth = currentTarget.GetComponent<HealthManager>();
        
        // If no HealthManager found, check on parent
        if (targetHealth == null)
        {
            targetHealth = currentTarget.GetComponentInParent<HealthManager>();
        }
        
        // If still no HealthManager, assume it's dead or invalid
        if (targetHealth == null)
        {
            Debug.LogWarning("Dog: Target has no HealthManager, treating as dead.");
            return true;
        }
        
        // Use the IsDead() method from HealthManager
        return targetHealth.IsDead();
    }
    
    // ========== THREAT DETECTION ==========
    void CheckForThreats()
    {
        // Only look for threats if not already attacking
        if (currentState == DogState.Attacking) return;
        if (player == null) return;
        
        // Find all zombies in range using Physics.OverlapSphere
        Collider[] zombiesInRange = Physics.OverlapSphere(transform.position, detectionRange, zombieLayer);
        
        if (zombiesInRange.Length > 0)
        {
            // Find the closest zombie
            Transform closestZombie = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Collider zombieCollider in zombiesInRange)
            {
                // Skip dead zombies
                HealthManager zombieHealth = zombieCollider.GetComponent<HealthManager>();
                if (zombieHealth == null)
                {
                    zombieHealth = zombieCollider.GetComponentInParent<HealthManager>();
                }
                
                // Only consider alive zombies (using IsDead method)
                if (zombieHealth != null && !zombieHealth.IsDead())
                {
                    float distance = Vector3.Distance(transform.position, zombieCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestZombie = zombieCollider.transform;
                    }
                }
            }
            
            // Only attack if zombie is threatening the player (within 8 units)
            if (closestZombie != null)
            {
                float zombieDistanceToPlayer = Vector3.Distance(closestZombie.position, player.position);
                
                if (zombieDistanceToPlayer < 8f)
                {
                    currentTarget = closestZombie;
                    currentState = DogState.Attacking;
                    Debug.Log("Dog: Detected threat! Attacking zombie.");
                }
            }
        }
    }
    
    // ========== ATTACK EXECUTION ==========
    void PerformBiteAttack()
    {
        Debug.Log("Dog: BITE!");
        
        // Trigger the bite animation
        if (animator != null)
        {
            animator.SetTrigger("Bite");
        }
    }
    
    // This will be called by Animation Event later
    public void EnableBiteHitbox()
    {
        // We'll implement this when we add the damage hitbox
        Debug.Log("Dog: Hitbox enabled (animation event)");
    }
    
    // ========== DEBUG VISUALIZATION ==========
    void OnDrawGizmosSelected()
    {
        // Yellow = Discovery range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, discoveryRange);
        
        // Red = Zombie detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Magenta = Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
