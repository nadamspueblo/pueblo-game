using UnityEngine;
using UnityEngine.AI;

public class RagdollManager : MonoBehaviour
{
  private Rigidbody[] boneRigidbodies;
  private Animator animator;
  private NavMeshAgent agent;

  [Header("Ragdoll Recovery")]
  public Transform hipsBone; // Drag the zombie's pelvis/hips bone here in the Inspector
  public float blendDuration = 0.5f;
  private class BoneTransform
  {
    public Vector3 position;
    public Quaternion rotation;
  }
  private BoneTransform[] storedBoneTransforms;
  private bool isBlending = false;
  private float blendTimer = 0f;

  [Tooltip("The main capsule collider used for movement, NOT the hitboxes")]
  private Collider mainCollider;

  void Awake()
  {
    // Initialize the storage array to match the number of bones
    boneRigidbodies = GetComponentsInChildren<Rigidbody>();
    storedBoneTransforms = new BoneTransform[boneRigidbodies.Length];
    for (int i = 0; i < storedBoneTransforms.Length; i++)
    {
      storedBoneTransforms[i] = new BoneTransform();
    }
  }
  void Start()
  {
    // 1. Gather all the components
    boneRigidbodies = GetComponentsInChildren<Rigidbody>();
    animator = GetComponentInChildren<Animator>();
    agent = GetComponent<NavMeshAgent>();
    mainCollider = GetComponent<Collider>();

    // Prevent the ragdoll from exploding itself ---
    Collider[] allRagdollColliders = GetComponentsInChildren<Collider>();
    foreach (Collider colA in allRagdollColliders)
    {
      foreach (Collider colB in allRagdollColliders)
      {
        // Make every collider on this zombie ignore every other collider on this zombie
        if (colA != colB)
        {
          Physics.IgnoreCollision(colA, colB);
        }
      }
    }
    // 2. Make sure the ragdoll is turned OFF when they spawn
    DisableRagdoll();
  }

  void LateUpdate()
  {
    // 6. The Magic Blend
    if (isBlending)
    {
      blendTimer += Time.deltaTime;
      float blendPercentage = blendTimer / blendDuration;

      if (blendPercentage >= 1f)
      {
        isBlending = false; // We are fully recovered!
        return;
      }

      // Loop through every bone and lerp it between the ragdoll pose and the Animator's current pose
      for (int i = 0; i < boneRigidbodies.Length; i++)
      {
        if (boneRigidbodies[i].gameObject == this.gameObject) continue;

        Transform boneTransform = boneRigidbodies[i].transform;

        boneTransform.localPosition = Vector3.Lerp(storedBoneTransforms[i].position, boneTransform.localPosition, blendPercentage);
        boneTransform.localRotation = Quaternion.Slerp(storedBoneTransforms[i].rotation, boneTransform.localRotation, blendPercentage);
      }
    }
  }

  public void StartRagdollRecovery()
  {
    // 1. Save the WORLD position and rotation of every ragdoll bone
    for (int i = 0; i < boneRigidbodies.Length; i++)
    {
      storedBoneTransforms[i].position = boneRigidbodies[i].transform.position;
      storedBoneTransforms[i].rotation = boneRigidbodies[i].transform.rotation;
    }

    Vector3 ragdollHipsWorldPos = hipsBone.position;

    // 2. Figure out the facing direction
    Vector3 hipsDirection = -hipsBone.up;
    hipsDirection.y = 0;

    // 3. Align the root's rotation to face the right way
    if (hipsDirection != Vector3.zero)
    {
      transform.rotation = Quaternion.LookRotation(hipsDirection.normalized);
    }

    // 4. Turn the Animator back on and freeze physics
    DisableRagdoll();

    // 5. Tell the Animator which animation to play
    if (hipsBone.up.y > 0)
    {
      animator.Play("Zombie Stand Up", 0, 0f);
    }
    else
    {
      animator.Play("Zombie Stand Up", 0, 0f);
    }

    // THE TRICK: Force the Animator to instantly calculate Frame 0 right now
    animator.Update(0f);

    // 6. Calculate the exact difference between the Ragdoll Hips and the Animated Hips
    Vector3 hipOffset = ragdollHipsWorldPos - hipsBone.position;
    hipOffset.y = 0; // Keep the root locked to the floor's height!

    // 7. Shift the root. Now the animated hips perfectly overlap the ragdoll hips!
    transform.position += hipOffset;

    // 8. With the root perfectly positioned, convert our saved WORLD coordinates
    // into the new LOCAL coordinates so the LateUpdate Lerp has the correct math.
    for (int i = 0; i < boneRigidbodies.Length; i++)
    {
      // Temporarily snap the bone back to its ragdoll world position
      boneRigidbodies[i].transform.position = storedBoneTransforms[i].position;
      boneRigidbodies[i].transform.rotation = storedBoneTransforms[i].rotation;

      // Now save its local position relative to the newly shifted root!
      storedBoneTransforms[i].position = boneRigidbodies[i].transform.localPosition;
      storedBoneTransforms[i].rotation = boneRigidbodies[i].transform.localRotation;
    }

    // 9. Start the blending process
    isBlending = true;
    blendTimer = 0f;
  }

  public void DisableRagdoll()
  {
    foreach (Rigidbody rb in boneRigidbodies)
    {
      // Safety check: Don't disable the root Rigidbody if you have one for the NavMesh!
      if (rb.gameObject == this.gameObject) continue;

      rb.isKinematic = true; // "Kinematic" means physics won't affect it
      rb.useGravity = false;
    }

    // Keep the main systems running
    if (animator != null) animator.enabled = true;
    if (agent != null)
    {
      agent.enabled = true;
      agent.updateRotation = true; // Fixes the "running wrong direction" bug!
      agent.ResetPath(); // Wipes old destination data
    }
    if (mainCollider != null) mainCollider.enabled = true;
  }

  public void EnableRagdoll()
  {
    // Turn off the brain, movement, and animation
    if (animator != null) animator.enabled = false;
    if (agent != null) agent.enabled = false;
    if (mainCollider != null) mainCollider.enabled = false;

    // Turn on the bone physics
    foreach (Rigidbody rb in boneRigidbodies)
    {
      if (rb.gameObject == this.gameObject) continue;

      rb.isKinematic = false; // Let physics take the wheel!
      rb.useGravity = true;
    }
  }
}