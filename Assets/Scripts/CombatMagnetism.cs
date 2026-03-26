using System.Collections;
using UnityEngine;

public class CombatMagnetism : MonoBehaviour
{
  [Header("Magnet Settings")]
  [Tooltip("How far away can we suck into a target?")]
  public float magnetRadius = 3.5f;
  [Tooltip("Maximum angle to target. 45 means a 90-degree front cone.")]
  public float maxAngle = 45f;
  [Tooltip("The perfect distance to stand from the target when the axe hits.")]
  public float optimalStrikeDistance = 1.2f;
  [Tooltip("How fast the slide happens (should match the wind-up of your animation).")]
  public float warpDuration = 0.15f;

  [Header("Targeting")]
  public LayerMask enemyLayer; // Set this to your EnemyHitbox layer in the Inspector!
  private CharacterController characterController;

  void Start()
  {
    characterController = GetComponent<CharacterController>();
  }

  // Your WeaponController will call this the exact moment you click Attack!
  public void TriggerMagnetism()
  {
    Transform bestTarget = FindBestTarget();
    if (bestTarget != null)
    {
      StartCoroutine(WarpToTarget(bestTarget));
    }
  }

  private Transform FindBestTarget()
  {
    // 1. Cast the invisible sphere
    Collider[] hits = Physics.OverlapSphere(transform.position, magnetRadius, enemyLayer);

    Transform bestTarget = null;
    float closestAngle = maxAngle;

    // 2. Filter the hits to find the one we are most directly facing
    foreach (Collider hit in hits)
    {
      // Get direction to this enemy, ignoring height differences
      Vector3 dirToTarget = hit.transform.position - transform.position;
      dirToTarget.y = 0;

      // Calculate the angle between where we are looking and where the enemy is
      float angleToTarget = Vector3.Angle(transform.forward, dirToTarget.normalized);

      // If it's inside our cone, and it's the most centered one we've found so far...
      if (angleToTarget < closestAngle)
      {
        closestAngle = angleToTarget;

        // Save the root transform (the zombie itself, not just the hand/head bone)
        bestTarget = hit.transform.root;
      }
    }

    return bestTarget;
  }

  private IEnumerator WarpToTarget(Transform target)
  {
    Vector3 startPos = transform.position;

    // Calculate exactly where we need to stand
    Vector3 dirToTarget = (target.position - transform.position);
    dirToTarget.y = 0; // Keep the math perfectly horizontal

    float currentDistance = dirToTarget.magnitude;

    // If we are already too close, don't warp backward!
    if (currentDistance <= optimalStrikeDistance) yield break;

    // The exact coordinates of our optimal strike zone
    Vector3 destination = target.position - (dirToTarget.normalized * optimalStrikeDistance);

    float elapsedTime = 0f;

    while (elapsedTime < warpDuration)
    {
      elapsedTime += Time.deltaTime;

      // Calculate a percentage (0.0 to 1.0) for our Lerp
      float percentComplete = elapsedTime / warpDuration;

      // SmoothStep makes the slide ease-in and ease-out like AAA games
      float smoothPercent = Mathf.SmoothStep(0f, 1f, percentComplete);
      
      // The destination dynamically shifts if the target is also moving toward us
      Vector3 currentDir = target.position - startPos;
      currentDir.y = 0;
      Vector3 dynamicDestination = target.position - (currentDir.normalized * optimalStrikeDistance);

      // Calculate where we should be THIS frame
      Vector3 nextPos = Vector3.Lerp(startPos, dynamicDestination, smoothPercent);

      // Calculate the tiny step to take, and feed it to the CharacterController
      Vector3 motionStep = nextPos - transform.position;
      if (characterController != null)
      {
        characterController.Move(motionStep);
      }

      // Bonus: Subtly rotate the player to perfectly face the target during the slide!
      Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothPercent * 2f);

      yield return null; // Wait for the next frame
    }
  }
}