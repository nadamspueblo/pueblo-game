using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Required to talk to the NavMeshAgent!

public class ZombieCombatMagnetism : MonoBehaviour
{
  [Header("Lunge Settings")]
  [Tooltip("The perfect distance for the zombie's teeth/claws to connect.")]
  public float optimalStrikeDistance = 1.0f;
  [Tooltip("How fast the lunge happens. Match this to the zombie's attack wind-up.")]
  public float warpDuration = 0.2f;

  private NavMeshAgent agent;
  private bool isWarping = false;
  private Coroutine activeMagnetismRoutine;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
  }

  // The Zombie's AI script will call this right as it triggers the attack animation
  public void LungeAtTarget(Transform target, float optimalStrikeDistance = -1.0f)
  {
    if (!isWarping && target != null && gameObject.activeInHierarchy)
    {
      CancelMagnetism(); 
      activeMagnetismRoutine = StartCoroutine(WarpToTarget(target, optimalStrikeDistance < 0 ? this.optimalStrikeDistance : optimalStrikeDistance));
    }
  }

  public void CancelMagnetism()
  {
    if (activeMagnetismRoutine != null)
        {
            StopCoroutine(activeMagnetismRoutine);
            activeMagnetismRoutine = null;
        }
  }

  private IEnumerator WarpToTarget(Transform target, float optimalStrikeDistance)
  {
    if (target == null) yield break;
    isWarping = true;
    Vector3 startPos = transform.position;

    Vector3 dirToTarget = target.position - transform.position;
    dirToTarget.y = 0;

    float currentDistance = dirToTarget.magnitude;

    // THE FIX 1: Unlock the door before we abort!
    if (currentDistance <= optimalStrikeDistance)
    {
      isWarping = false;
      yield break;
    }

    Vector3 destination = target.position - (dirToTarget.normalized * optimalStrikeDistance);

    if (agent != null && agent.isOnNavMesh)
    {
      agent.isStopped = true;
    }

    RootMotionAnimation anim = GetComponent<RootMotionAnimation>();
    if (anim != null) anim.ignoreRootMotion = true;

    float elapsedTime = 0f;
    Vector3 lastPos = startPos;

    while (elapsedTime < warpDuration)
    {
      if (target == null) break;
      elapsedTime += Time.deltaTime;
      float percentComplete = elapsedTime / warpDuration;
      float smoothPercent = Mathf.SmoothStep(0f, 1f, percentComplete);

      // The destination dynamically shifts if the target is also moving toward us
      Vector3 currentDir = target.position - startPos;
      currentDir.y = 0;
      Vector3 dynamicDestination = target.position - (currentDir.normalized * optimalStrikeDistance);

      Vector3 nextPos = Vector3.Lerp(startPos, dynamicDestination, smoothPercent);
      Vector3 motionStep = nextPos - lastPos;

      // THE FIX 2: Brute force BOTH the NavMeshAgent and the Physical Transform
      if (agent != null && agent.isOnNavMesh)
      {
        agent.Move(motionStep);
      }
      // Manually drag the visual mesh just in case Root Motion decoupled it!
      transform.position += motionStep;

      lastPos = nextPos;

      Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothPercent * 2f);

      yield return null;
    }

    if (anim != null) anim.ignoreRootMotion = false;
    if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

    isWarping = false;
  }
}