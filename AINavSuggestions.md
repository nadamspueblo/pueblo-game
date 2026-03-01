# Useful agent Tools for Your State Machine
Since you want to create distinct behaviors (like a slow, creepy walk when wandering, but a terrifying sprint when chasing), here are the most powerful properties built into the ```NavMeshAgent``` that you can use in  ```NPCTemplate```:

1. Path Distance 
Right now, you are using ```Vector3.Distance``` to check how far the zombie is from the player. That calculates a straight line through walls.
If the player is hiding on the other side of a brick wall, ```Vector3.Distance``` might say they are only 2 meters away, so the zombie will try to bite the brick wall!

Instead, you can use the agent's internal path distance:

```csharp
// Drop this function anywhere inside your StudentNPCTemplate class
private float GetActualPathDistance(Vector3 targetPosition)
{
    NavMeshPath path = new NavMeshPath();
    
    // Ask the NavMesh to draw a path from the zombie to the target
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
    
    // If there is no valid path (e.g. player is on a roof), return infinity
    return Mathf.Infinity; 
}
```
```csharp
void Update()
{
    // 1. CHEAP CHECK: Is the player even close enough to care about?
    float directDistance = Vector3.Distance(transform.position, targetToChase.position);

    if (directDistance <= chaseDistance)
    {
        // 2. ACCURATE CHECK: How far is the actual walking path around the desks/walls?
        float trueWalkingDistance = GetActualPathDistance(targetToChase.position);

        if (trueWalkingDistance <= attackDistance)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackTarget();
            }
            else
            {
                agent.isStopped = true;
                FaceTarget();
                animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
            }
        }
        else if (trueWalkingDistance <= chaseDistance)
        {
            ChaseTarget();
        }
    }
    else
    {
        WanderAround();
    }
}
```
2. ```agent.ResetPath()```
If the player runs inside a safe zone and shuts the door, you don't want the zombie to just moonwalk against the door forever. You can tell the GPS to cancel its current route.

```csharp
// Player escaped! Cancel the chase.
agent.ResetPath(); 

// Tell the Animator to return to Idle (0 speed)
animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
```
3. ```agent.pathStatus```
You can actually ask the agent if it's impossible to reach the player (for example, if the player jumped onto a table that the NavMesh doesn't reach).

```csharp
if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
{
    // The zombie realizes it can't reach the player!
    // Maybe play a frustrated "Roar" animation, or go back to wandering.
}
```
How to Transition Between Walk and Run
Because you are using Root Motion, the zombie's speed is determined entirely by the ```VelocityZ``` float in your Animator's Blend Tree.

Right now, your script passes ```agent.desiredVelocity.magnitude``` directly to the Animator. But you can manually override this to force specific animations based on the AI's current "State":

```csharp
// Inside your ChaseTarget() function:
// Force the Animator into the "Run" threshold (e.g., 3.5)
animator.SetFloat("VelocityZ", 3.5f, 0.1f, Time.deltaTime); 
agent.SetDestination(targetToChase.position);

// Inside your WanderAround() function:
// Force the Animator into the "Walk" threshold (e.g., 1.5)
animator.SetFloat("VelocityZ", 1.5f, 0.1f, Time.deltaTime);
agent.SetDestination(randomWanderPoint);
```