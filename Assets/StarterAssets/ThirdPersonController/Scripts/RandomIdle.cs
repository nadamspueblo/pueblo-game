using UnityEngine;

public class RandomIdle : StateMachineBehaviour
{
    [Header("Idle Setup")]
    [Tooltip("The exact name of the Int parameter in your Animator")]
    public string idleIndexParam = "IdleIndex";
    
    [Tooltip("How many special idle animations do you have?")]
    public int numberOfSpecialIdles = 3;

    [Header("Timing")]
    public float minWaitTime = 5f;
    public float maxWaitTime = 12f;

    private float _timer;
    private float _waitTime;

    // OnStateEnter is called the exact frame the Base Idle starts playing
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. Reset the parameter so we don't accidentally loop a special idle
        animator.SetInteger(idleIndexParam, 0);
        
        // 2. Reset the timer and pick a new random wait time
        _timer = 0f;
        _waitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    // OnStateUpdate is called every single frame while the Base Idle is playing
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Only count up if we are actually resting in the idle (not transitioning in/out)
        if (!animator.IsInTransition(layerIndex))
        {
            _timer += Time.deltaTime;

            if (_timer >= _waitTime)
            {
                // Pick a random number between 1 and your max number of idles
                int randomIdle = Random.Range(1, numberOfSpecialIdles + 1);
                
                // Send that number to the Animator to trigger the transition!
                animator.SetInteger(idleIndexParam, randomIdle);
            }
        }
    }
}