using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    [Header("Debug")]
    public bool showNoiseRadius = true;
    public float debugRadius = 10f; // Just for drawing in the Scene view

    // Call this from your player's movement script or when firing a weapon!
    public void MakeNoise(float noiseLevel)
    {
        // 1. Find all physics colliders within the noise radius
        // (In a massive multiplayer game, you would add a LayerMask here to ONLY check the Enemy layer for performance)
        Collider[] colliders = Physics.OverlapSphere(transform.position, noiseLevel);

        // 2. Loop through everything the sphere touched
        foreach (Collider col in colliders)
        {
            // 3. Does this object have our advanced zombie AI?
            // We use GetComponentInParent just in case the sphere hits a body part hitbox instead of the root capsule
            ZombieAdvancedAI zombie = col.GetComponentInParent<ZombieAdvancedAI>();
            
            if (zombie != null)
            {
                // 4. Tell the zombie exactly where the sound came from and how loud it was
                zombie.HearSound(transform.position, noiseLevel);
            }
        }
    }

    // This lets you visually see the noise radius in the Scene view (not the game view) when the player is selected
    private void OnDrawGizmosSelected()
    {
        if (showNoiseRadius)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Transparent yellow
            Gizmos.DrawWireSphere(transform.position, debugRadius); 
        }
    }
}