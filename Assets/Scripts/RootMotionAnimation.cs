using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class RootMotionAnimation : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        // 1. Tell the NavMeshAgent to calculate the path, but stop pushing the body forward
        agent.updatePosition = false;
    }

    // This built-in Unity function runs every frame right after the animation plays
    void OnAnimatorMove()
    {
        // 2. Look at the animation and find exactly where the foot stepped this frame
        Vector3 newPosition = anim.rootPosition;
        
        // 3. Make sure the foot stays locked to the height of the blue NavMesh floor
        newPosition.y = agent.nextPosition.y; 
        
        // 4. Physically move the 3D model to the new footstep location
        transform.position = newPosition; 
        
        // 5. Tell the invisible NavMeshAgent brain to catch up to where the body just stepped
        agent.nextPosition = transform.position; 
    }
}