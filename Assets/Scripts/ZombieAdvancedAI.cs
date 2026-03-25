using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAdvancedAI : MonoBehaviour
{
  public enum ZombieState { Wander, Investigate, Chase, Attack, Feeding, Scream, Unconscious, StandUp, Crawling, Circling, QuickBite, Dead }

  [Header("References")]
  public AttackController playerAttackController;

  [Header("Current State")]
  public ZombieState currentState = ZombieState.Wander;

  [Header("Senses & Components")]
  public float viewDistance = 15f;
  public float fieldOfView = 90f;
  public LayerMask sightBlockers;
  public float hearingSensitivity = 20f;
  public NoiseMaker noiseMaker;
  public float alertScreamRadius = 30f; // How far the scream travels
  public RagdollManager ragdollManager;
  public SurvivalStats survivalStats;
  public PlayerCombatReactions combatReactions;

  [Header("Combat & Movement")]
  public float wanderRadius = 10f;
  public float wanderTimer = 5f;
  private float timer;
  [Range(0.0f, 10.0f)]
  public float walkSpeed = 5f;
  [Range(0.0f, 10.0f)]
  public float chaseSpeed = 10f;
  public float attackDistance = 1.5f;
  public float attackCooldown = 2f;
  private float lastAttackTime;
  public float biteCooldown = 10f;
  private float lastBiteTime;
  private bool isGrappleBroken = false; // Breaks the bite grapple when true
  public float circlingRadius = 2.3f;
  public float maxCirclingTime = 3f; // How long they will try to circle before giving up
  private float circlingTimer = 0f;
  public ZombieCombatMagnetism combatMagnetism;

  [Header("Crowd Control")]
  public float separationRadius = 0.8f; // How close they can get before pushing
  public float separationForce = 1.5f;

  [Header("Head Tracking")]
  public Transform headBone; // Drag the Mixamo head/neck bone here
  public float headLookSpeed = 5f;
  public float maxHeadTurnAngle = 70f; // Don't let them snap their necks!
  private float currentLookWeight = 0f;

  [Header("State Timers")]
  public float minimumUnconsciousTime = 5f; // Time before it CAN be woken up
  private float currentUnconsciousTimer = 0f;
  private float standUpTime = 3f;
  private float lastStandUptime = -1f;

  [Header("Audio Setup")]
  public AudioSource audioSource;
  public AudioClip alertSound;
  public float alertCooldown = 5f;
  private float lastAlertTime = -5f;
  private bool hasAlerted = false;
  public AudioClip screamSound;
  public AudioClip attackSound; // Renamed from attachSound for clarity
  public AudioClip[] idleGrowls;

  private float growlTimer = 0f;
  private float nextGrowlTime = 5f;

  private NavMeshAgent agent;
  private Animator animator;
  private Transform player;
  private HealthManager healthManager;

  private Vector3 investigationPoint;
  private float speed = 0f;

  private Collider[] allZombieColliders;
  private Collider playerCollider;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponentInChildren<Animator>();
    healthManager = GetComponent<HealthManager>();
    player = GameObject.FindWithTag("Player").transform;
    playerAttackController = player.GetComponent<AttackController>();

    timer = wanderTimer;
    SetNextGrowlTime();
    SetState(currentState);

    // Randomly vary which animation frame the zombie starts at for variation
    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
    animator.Play(state.fullPathHash, 0, Random.Range(0f, 1f));

    // Collider references for ignoring physics
    allZombieColliders = GetComponentsInChildren<Collider>();
    playerCollider = player.GetComponent<Collider>();

    // Survival stats for dealing direct damage from bites
    survivalStats = player.GetComponent<SurvivalStats>();

    // Combat reactions
    combatReactions = player.GetComponent<PlayerCombatReactions>();

    // Random avoidance priorities to help with avoidance logic
    if (agent != null)
    {
      agent.avoidancePriority = Random.Range(0, 100);
    }
  }

  void Update()
  {
    if (healthManager != null && healthManager.IsDead() || currentState == ZombieState.Dead) return;

    HandleCrowdSeparation();

    // 1. Always check our sensors first
    CheckSenses();

    // 2. Act based on our current state
    switch (currentState)
    {
      case ZombieState.Wander: UpdateWanderState(); break;
      case ZombieState.Investigate: UpdateInvestigateState(); break;
      case ZombieState.Chase: UpdateChaseState(); break;
      case ZombieState.Attack: UpdateAttackState(); break;
      case ZombieState.Feeding: UpdateFeedingState(); break;
      case ZombieState.Scream: UpdateScreamState(); break;
      case ZombieState.Unconscious: UpdateUnconsciousState(); break;
      case ZombieState.StandUp: UpdateStandUpState(); break;
      case ZombieState.Crawling: UpdateCrawlingState(); break;
      case ZombieState.Circling: UpdateCirclingState(); break;
    }

    // 3. Update the Animator
    if (animator != null)
    {
      animator.SetFloat("VelocityZ", speed, 0.1f, Time.deltaTime);
    }
  }

  void OnAnimatorIK(int layerIndex)
  {
    if (animator == null || player == null) return;

    // 1. Should we be looking at the player?
    bool shouldLook = (currentState == ZombieState.Chase || currentState == ZombieState.Circling || currentState == ZombieState.Attack);

    // 2. Is the player actually in front of us?
    Vector3 directionToPlayer = player.position - transform.position;
    float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

    // 3. Smoothly blend the head turn in and out (1 = looking, 0 = not looking)
    float targetWeight = (shouldLook && angleToPlayer < 70f) ? 1f : 0f;
    currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * headLookSpeed);

    // 4. Let Unity handle the complicated bone math!
    // We add Vector3.up * 1.5f so the zombie looks at the player's face, not their feet
    animator.SetLookAtPosition(player.position + Vector3.up * 1.5f);

    // The parameters are: (Total Weight, Body Weight, Head Weight, Eyes Weight, Clamp Weight)
    // Clamp Weight prevents the Exorcist neck-snap!
    animator.SetLookAtWeight(currentLookWeight, 0.2f, 0.8f, 1f, 0.5f);

  }

  private void OnTriggerEnter(Collider other)
  {
    if (currentState == ZombieState.Dead) return;
    // If the zombie is trying to maneuver around the player...
    if (other.CompareTag("Player"))
    {
      if (Time.time >= lastBiteTime + biteCooldown && !playerAttackController.isAttacking)
      {
        if (currentState == ZombieState.Circling || currentState == ZombieState.Chase)
        {
          ChangeState(ZombieState.QuickBite);
        }
        if (currentState == ZombieState.Attack && Random.value > 0.5f)
        {
          ChangeState(ZombieState.QuickBite);
        }
        else
        {
          ChangeState(ZombieState.Wander);
        }
      }
      else
      {
        ChangeState(ZombieState.Wander);
      }
    }
  }

  // --- SENSORS ---

  private void CheckSenses()
  {
    if (currentState == ZombieState.Attack || currentState == ZombieState.Unconscious || currentState == ZombieState.StandUp || currentState == ZombieState.Scream) return;

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    // Wake up from Unconscious via physical contact (Proximity)
    if (currentState == ZombieState.Unconscious && currentUnconsciousTimer <= 0f && distanceToPlayer <= 1.5f)
    {
      ChangeState(ZombieState.StandUp); // Animation event sets next state to Wander
      return;
    }

    // Alert player they are within hearing distance
    if (distanceToPlayer <= hearingSensitivity)
    {
      if (!hasAlerted && Time.time >= lastAlertTime + alertCooldown)
      {
        PlayAudio(alertSound);
        lastAlertTime = Time.time;
        hasAlerted = true;
      }
    }
    else
    {
      hasAlerted = false;
    }

    // Standard Vision Check
    if (distanceToPlayer <= viewDistance)
    {


      ChangeState(ZombieState.Chase);
      Vector3 directionToPlayer = (player.position - transform.position).normalized;
      float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

      if (angleToPlayer <= fieldOfView / 2f && !Physics.Linecast(transform.position + Vector3.up * 1.5f, player.position + Vector3.up * 1.5f, sightBlockers))
      {
        if (currentState == ZombieState.Feeding || currentState == ZombieState.Wander)
        {
          ChangeState(ZombieState.Scream); // See player -> Scream -> Chase
        }
        else
        {
          ChangeState(ZombieState.Chase);
        }
      }
    }
  }

  public void HearSound(Vector3 soundLocation, float noiseLevel)
  {
    if (currentState == ZombieState.Dead) return;
    // 1. Can we wake up from being unconscious?
    if (currentState == ZombieState.Unconscious)
    {
      if (currentUnconsciousTimer <= 0f)
      {
        ChangeState(ZombieState.StandUp);
      }
      return; // Don't process investigation logic if we are waking up
    }

    if (currentState == ZombieState.Chase || currentState == ZombieState.Attack || currentState == ZombieState.StandUp || currentState == ZombieState.Scream) return;

    float distanceToSound = Vector3.Distance(transform.position, soundLocation);
    if (distanceToSound <= noiseLevel * hearingSensitivity)
    {
      investigationPoint = soundLocation;

      // If we are feeding, wake up and scream first!
      if (currentState == ZombieState.Feeding)
      {
        ChangeState(ZombieState.Scream);
      }
      else
      {
        ChangeState(ZombieState.Investigate);
      }
    }
  }

  // --- STATE MACHINE LOGIC ---
  private void ChangeState(ZombieState newState)
  {
    if (currentState == newState) return;
    SetState(newState);
  }
  private void SetState(ZombieState newState)
  {
    currentState = newState;

    switch (newState)
    {
      case ZombieState.Scream:
        agent.isStopped = true;
        animator.SetTrigger("Scream");
        break;
      case ZombieState.Unconscious:
        agent.isStopped = true;
        currentUnconsciousTimer = minimumUnconsciousTime;
        ragdollManager.EnableRagdoll();
        break;
      case ZombieState.StandUp:
        ragdollManager.StartRagdollRecovery();
        lastStandUptime = Time.time;
        break;
      case ZombieState.Crawling:
        animator.SetBool("IsCrawling", true);
        break;
      case ZombieState.Feeding:
        agent.isStopped = true;
        animator.SetBool("IsFeeding", true);
        break;
      case ZombieState.Chase:
        animator.SetBool("IsFeeding", false);
        agent.isStopped = false;
        speed = Random.Range(0.85f * chaseSpeed, 1.15f * chaseSpeed);
        break;
      case ZombieState.Circling:
        agent.isStopped = false;
        speed = Random.Range(0.85f * chaseSpeed, 1.15f * chaseSpeed);
        agent.SetDestination(GetCirclingPoint(2f, Random.value > 0.5f));
        break;
      case ZombieState.QuickBite:
        animator.ResetTrigger("Attack");
        PlayerCombatReactions combatReactions = player.GetComponent<PlayerCombatReactions>();
        if (!combatReactions.isGrappled)
        {
          StartCoroutine(ExecuteGrappleAttack());
        }
        break;
      case ZombieState.Wander:
      case ZombieState.Investigate:
        agent.isStopped = false;
        speed = Random.Range(0.85f * walkSpeed, 1.15f * walkSpeed);
        break;
      case ZombieState.Dead:
        StartCoroutine(TurnIntoStaticCorpse());
        break;
    }
  }

  private void UpdateFeedingState() { /* Remains still, waiting for CheckSenses or HearSound */ }
  private void UpdateScreamState() { /* Remains still, waiting for Animation Event to finish */ }
  private void UpdateStandUpState()
  {
    /* Remains still, waiting for Animation Event to finish */
    // If animation event doesn't trigger Wander state, the timer will
    if (Time.time > lastStandUptime + standUpTime)
    {
      ChangeState(ZombieState.Wander);
    }
  }

  private void UpdateUnconsciousState()
  {
    if (currentUnconsciousTimer > 0)
    {
      currentUnconsciousTimer -= Time.deltaTime;
    }
  }

  private void UpdateCrawlingState()
  {
    // Crawling acts just like Chase, but drastically slower
    agent.speed = walkSpeed * 0.5f;
    agent.SetDestination(player.position);
    if (Vector3.Distance(transform.position, player.position) <= attackDistance)
    {
      ChangeState(ZombieState.Attack);
    }
  }

  private void UpdateWanderState()
  {
    HandleIdleGrowls(); //

    timer += Time.deltaTime; //

    if (timer >= wanderTimer) //
    {
      Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1); //
      agent.SetDestination(newPos); //
      timer = 0; //
    }
  }

  private void UpdateInvestigateState()
  {
    HandleIdleGrowls(); // Keep growling while looking for the noise!

    agent.SetDestination(investigationPoint);

    if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
    {
      ChangeState(ZombieState.Wander);
    }
  }

  private void UpdateChaseState()
  {
    agent.SetDestination(player.position);
    float distToPlayer = Vector3.Distance(transform.position, player.position);

    if (distToPlayer <= attackDistance)
    {
      ChangeState(ZombieState.Attack);
      speed = 0.9f * chaseSpeed;
    }
    else if (distToPlayer > attackDistance)
    {
      speed = chaseSpeed;
    }
    else if (distToPlayer > viewDistance * 1.5f)
    {
      investigationPoint = player.position;
      ChangeState(ZombieState.Investigate);
    }
  }

  private void UpdateAttackState()
  {
    agent.isStopped = true;
    animator.speed = 1f;

    if (Time.time >= lastAttackTime + attackCooldown && !playerAttackController.isAttacking)
    {
      // Attack variation
      float value = Random.value;
      if (value < 0.3)
      {
        // Walk around player
        ChangeState(ZombieState.Circling);
      }
      else
      {
        FaceTarget();
        PlayAudio(attackSound);
        animator.SetTrigger("Attack");
        if (Random.value > 0.33)
        {
          animator.SetInteger("AttackIndex", Random.Range(0, 4));
        }
        else
        {
          animator.SetInteger("AttackIndex", Random.Range(4, 7));
        }

        if (combatMagnetism != null)
        {
          combatMagnetism.LungeAtTarget(player);
        }
      }

      lastAttackTime = Time.time;
    }

    if (Vector3.Distance(transform.position, player.position) > attackDistance)
    {
      agent.isStopped = false;
      ChangeState(ZombieState.Chase);
    }
  }

  private void UpdateCirclingState()
  {
    // 1. Keep staring at the player! 
    // Since the agent is moving to a point beside the player, forcing them to 
    // face the player creates a very creepy "strafing" or side-stepping look.
    //FaceTarget(); 

    circlingTimer += Time.deltaTime;

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    // We use Vector3.Distance to the destination instead of agent.remainingDistance 
    // because remainingDistance can sometimes falsely report 0 on complex path corners.
    float distanceToDestination = Vector3.Distance(transform.position, agent.destination);

    // 2. Did the player run away while we were trying to circle them?
    if (distanceToPlayer > circlingRadius * 1.5f)
    {
      // Player broke the engagement ring. Go back to a full chase!
      ChangeState(ZombieState.Chase);
      return;
    }

    // 3. Have we arrived at the flanking spot? OR did we run out of time?
    // (The timer prevents them from getting stuck if the NavMesh point is behind a barrel)
    if (distanceToDestination <= attackDistance || circlingTimer >= maxCirclingTime)
    {
      // We are in position. Time to strike!
      ChangeState(ZombieState.Attack);
    }
  }

  // --- HELPER FUNCTIONS ---

  // Leaves zombie corpose but strips all components to free resources
  private IEnumerator TurnIntoStaticCorpse()
  {
    // 1. Give the ragdoll 5 seconds to fall, bounce, and settle on the floor
    yield return new WaitForSeconds(3f);

    RootMotionAnimation rootMotion = GetComponent<RootMotionAnimation>();
    Destroy(rootMotion);

    // 2. STRIP JOINTS (CRITICAL ORDER)
    // The Unity Ragdoll Wizard uses CharacterJoints to connect the bones. 
    // You MUST destroy joints before rigidbodies, or Unity will throw severe console errors!
    CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>();
    foreach (CharacterJoint joint in joints)
    {
      Destroy(joint);
    }

    // 3. STRIP RIGIDBODIES
    // This completely removes the corpse from the PhysX engine's calculations.
    Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
    foreach (Rigidbody rb in rigidbodies)
    {
      Destroy(rb);
    }

    // 4. STRIP COLLIDERS
    // Removes the hitboxes so the player and other zombies can walk right over the body.
    Collider[] colliders = GetComponentsInChildren<Collider>();
    foreach (Collider col in colliders)
    {
      Destroy(col);
    }

    // 5. STRIP HEAVY COMPONENTS
    // Delete the Animator and NavMeshAgent to free up CPU memory
    Animator animator = GetComponentInChildren<Animator>();
    if (animator != null) Destroy(animator);

    RagdollManager ragdoll = GetComponent<RagdollManager>();
    if (ragdoll != null) Destroy(ragdoll);

    // Optional: Destroy the health manager or any other custom scripts here
    // HealthManager health = GetComponent<HealthManager>();
    // if (health != null) Destroy(health);

    // 6. FINALLY: Destroy this exact AI script so it never runs Update() again
    Destroy(this);

    NavMeshAgent agent = GetComponent<NavMeshAgent>();
    if (agent != null) Destroy(agent);
  }

  // Sinks zombie corpse into the floor after 10 seconds
  private IEnumerator SinkIntoFloor()
  {
    // 1. Let the zombie lie dead on the floor for a few seconds
    yield return new WaitForSeconds(10f);

    // 2. Freeze the ragdoll! 
    // We must make the bones kinematic again so they stop fighting the teleport and 
    // simply follow the root object as it moves underground.
    Rigidbody[] allBones = GetComponentsInChildren<Rigidbody>();
    foreach (Rigidbody rb in allBones)
    {
      rb.isKinematic = true;
      rb.useGravity = false;
    }

    // 3. Turn off every single collider to ensure they don't trip the player 
    // or trigger any weird physics glitches as they clip through the floor.
    Collider[] allColliders = GetComponentsInChildren<Collider>();
    foreach (Collider col in allColliders)
    {
      col.enabled = false;
    }

    // 4. The Sink Loop
    float sinkSpeed = 0.5f; // How fast they sink (units per second)
    float sinkTimer = 0f;
    float sinkDuration = 4f; // How long they sink before popping out of existence

    while (sinkTimer < sinkDuration)
    {
      sinkTimer += Time.deltaTime;

      // Push the entire zombie down in World Space
      transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);

      yield return null;
    }

    // 5. The cleanup is complete. Free up the memory!
    Destroy(gameObject);
  }

  private void HandleCrowdSeparation()
  {
    // Don't push each other if we are dead, biting, or getting up!
    if (currentState == ZombieState.Unconscious || currentState == ZombieState.QuickBite || currentState == ZombieState.Attack) return;

    // Draw an invisible sphere around the zombie. 
    // It returns an array of every collider inside that sphere.
    Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius);

    foreach (Collider neighbor in neighbors)
    {
      // 1. Don't push ourselves!
      if (neighbor.gameObject == this.gameObject) continue;

      // 2. Is the thing in our bubble another zombie?
      if (neighbor.CompareTag("Enemy"))
      {
        // 3. Calculate the push away
        Vector3 pushDirection = transform.position - neighbor.transform.position;
        pushDirection.y = 0; // Keep them on the floor

        // 4. Shove!
        transform.position += pushDirection.normalized * separationForce * Time.deltaTime;
      }
    }
  }

  private IEnumerator ExecuteGrappleAttack()
  {
    lastBiteTime = Time.time;
    // 1. Lock the movement systems
    RootMotionAnimation rootMotion = GetComponent<RootMotionAnimation>();
    if (rootMotion != null) rootMotion.ignoreRootMotion = true;
    if (agent != null) agent.isStopped = true;

    ToggleIgnoreCollision(true);

    // 2. The Initial Lunge (Slide into position over 0.15s)
    float slideTimer = 0f;
    Vector3 startPos = transform.position;
    Quaternion startRot = transform.rotation;

    while (slideTimer < 0.15f)
    {
      slideTimer += Time.deltaTime;
      float percent = slideTimer / 0.15f;

      // Calculate dynamically in case the player is already moving
      Vector3 targetPos = player.position + (player.forward * 0.35f);
      targetPos.y = transform.position.y;
      Quaternion targetRot = Quaternion.LookRotation(player.position - transform.position);

      transform.position = Vector3.Lerp(startPos, targetPos, percent);
      transform.rotation = Quaternion.Slerp(startRot, targetRot, percent);
      yield return null;
    }

    // 3. We are in position. Play the animation!
    isGrappleBroken = false;
    animator.SetTrigger("NeckBite");

    // 4. THE STICKY PHASE (Hold on for the duration of the animation)
    float biteDuration = 4.07f; // Adjust to your specific clip length
    float biteTimer = 0f;
    bool hasTriggeredReaction = false;

    // Instead of WaitForSeconds, we run a loop every frame while the animation plays
    while (biteTimer < biteDuration)
    {
      // Start the players reaction after the attack has already begun
      if (combatReactions != null && !hasTriggeredReaction)
      {
        hasTriggeredReaction = true;
        combatReactions.ReactToGrapple(1.3f, this);
      }

      if (isGrappleBroken)
      {
        // Trigger a stumble/shoved animation on the zombie
        animator.SetTrigger("StaggerBack");

        // Break completely out of the while loop right now!
        break;
      }

      biteTimer += Time.deltaTime;

      // Constantly recalculate the perfect offset
      Vector3 stickyPos = player.position + (player.forward * 0.35f);
      stickyPos.y = transform.position.y; // Keep grounded

      // Constantly stare at the player
      Quaternion stickyRot = Quaternion.LookRotation(player.position - transform.position);

      // Use a tight Lerp here instead of snapping. 
      // If another zombie shoves the player, this makes the biting zombie 
      // look like it is being dragged along with the player!
      transform.position = Vector3.Lerp(transform.position, stickyPos, Time.deltaTime * 15f);
      transform.rotation = Quaternion.Slerp(transform.rotation, stickyRot, Time.deltaTime * 15f);

      yield return null; // Wait for the next frame
    }

    if (!isGrappleBroken)
    {
      // They didn't escape in time. Apply the damage!
      //survivalStats.TakeDamage(50f, transform);
      combatReactions.EndBiteGrapple();
    }

    // Cleanup & Release
    ToggleIgnoreCollision(false);

    if (rootMotion != null) rootMotion.ignoreRootMotion = false;
    if (agent != null) agent.isStopped = false;

    // 4. Change state based on the outcome
    if (isGrappleBroken)
    {
      // Give the player a second to breathe if they escaped
      ChangeState(ZombieState.Chase); // Or create a specific "Stunned" state!
    }
    else
    {
      ChangeState(ZombieState.Chase);
    }
  }

  private void ToggleIgnoreCollision(bool ignoreCollision)
  {
    if (playerCollider == null) return;

    foreach (Collider col in allZombieColliders)
    {
      // We only need to ignore solid collisions, not triggers
      if (col != null && !col.isTrigger)
      {
        Physics.IgnoreCollision(col, playerCollider, ignoreCollision);
      }
    }
  }
  private Vector3 GetCirclingPoint(float radius, bool goRight)
  {
    // 1. Get the direction from the PLAYER to the ZOMBIE
    Vector3 dirFromPlayerToZombie = (transform.position - player.position).normalized;

    // 2. Decide the angle. 75 to 90 degrees creates a great flanking arc.
    float angle = goRight ? 75f : -75f;

    // 3. Rotate that direction vector around the Y axis
    Vector3 rotatedDirection = Quaternion.Euler(0, angle, 0) * dirFromPlayerToZombie;

    // 4. Calculate the ideal point in world space
    Vector3 idealPoint = player.position + (rotatedDirection * radius);

    // 5. CRITICAL: Ask the NavMesh if this ideal point is actually walkable
    // (We don't want the zombie trying to circle inside a solid wall)
    NavMeshHit hit;
    if (NavMesh.SamplePosition(idealPoint, out hit, 3f, NavMesh.AllAreas))
    {
      return hit.position; // Found a valid spot on the floor!
    }

    // Fallback: If the side is blocked by a wall, just hold current position
    return transform.position;
  }

  private void FaceTarget()
  {
    Vector3 direction = (player.position - transform.position).normalized;
    direction.y = 0;
    Quaternion lookRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
  }

  public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
  {
    Vector3 randDirection = Random.insideUnitSphere * dist;
    randDirection += origin;

    NavMeshHit navHit;
    NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

    return navHit.position;
  }

  private void HandleIdleGrowls()
  {
    growlTimer += Time.deltaTime;
    if (growlTimer >= nextGrowlTime)
    {
      if (idleGrowls.Length > 0)
      {
        int randomIndex = Random.Range(0, idleGrowls.Length);
        PlayAudio(idleGrowls[randomIndex]);
      }
      SetNextGrowlTime();
    }
  }

  private void SetNextGrowlTime()
  {
    growlTimer = 0f;
    nextGrowlTime = Random.Range(4f, 10f);
  }

  // Consolidated audio player to keep the script clean
  private void PlayAudio(AudioClip clip)
  {
    if (clip != null && audioSource != null) //
    {
      audioSource.pitch = Random.Range(0.8f, 1.2f); //
      audioSource.PlayOneShot(clip); //
    }
  }

  // --- ANIMATION EVENTS ---

  // TODO: Call in the Scream animation when the mouth opens
  public void Event_TriggerScreamAndNoiseMaker()
  {
    PlayAudio(screamSound); // From your helper method
    if (noiseMaker != null)
    {
      noiseMaker.MakeNoise(alertScreamRadius); // Wakes up other zombies!
    }
  }

  public void Event_ChangeState(string stateName)
  {
    // TryParse is safe; it won't crash your game if you make a typo in the Animation window
    if (System.Enum.TryParse(stateName, out ZombieState nextState))
    {
      ChangeState(nextState);
    }
    else
    {
      Debug.LogError("Animation Event Error: ZombieState '" + stateName + "' does not exist!");
    }
  }

  // TODO: Implement quicktime/slowmotion during grapple and break grapple logic in ThirdPersonController
  public void BreakGrapple()
  {
    isGrappleBroken = true;
    animator.ResetTrigger("NeckBite");
    animator.SetTrigger("FallBack");
  }

  public void Event_Bite()
  {
    survivalStats.TakeDamage(40f, transform);
    combatReactions.ReactToBite();
  }
}