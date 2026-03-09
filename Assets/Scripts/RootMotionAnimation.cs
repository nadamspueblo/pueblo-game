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
        // 1. THE LEASH: Yank the GPS marker back to exactly where the physical body is.
        // This completely erases any automatic forward movement the AI tried to add this frame!
        agent.nextPosition = transform.position;
        
        // 2. Ask the animation exactly how far the foot stepped
        Vector3 step = anim.deltaPosition;
        
        // 3. Command the AI to take that exact step.
        // Because we are using agent.Move, it will mathematically stop if it hits a fence!
        agent.Move(step);
        
        // 4. Snap the 3D model to the safe, wall-checked position
        transform.position = agent.nextPosition;
    }
}