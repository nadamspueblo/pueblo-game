using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
  [Header("Audio Setup")]
  public AudioSource audioSource;
  public AudioClip impactSound;
  public float damageAmount = 25f;
  private Collider hitboxCollider;

  void Start()
  {
    hitboxCollider = GetComponent<Collider>();
    // Start the game with the weapon turned OFF so we don't deal accidental damage
    hitboxCollider.enabled = false;
  }

  // The Animation Event will call this to turn the weapon ON
  public void EnableHitbox()
  {
    hitboxCollider.enabled = true;
  }

  // The Animation Event will call this to turn the weapon OFF
  public void DisableHitbox()
  {
    hitboxCollider.enabled = false;
  }

  void OnTriggerEnter(Collider other)
  {
    
    // 1. Make sure we don't accidentally hit ourselves
    if (other.transform.root == transform.root) return;
    PlayImpactSound();

    // 2. Check if the thing we hit has health
    if (other.CompareTag("Player")) {
      SurvivalStats playerStats = other.GetComponent<SurvivalStats>();

      if (playerStats != null)
      {
        playerStats.TakeDamage(damageAmount);

        // 3. Immediately turn off the hitbox so it doesn't hit the zombie 5 times in one frame
        DisableHitbox();
      }
    }
    else {
      Debug.Log("Hit enemy");
      HealthManager targetHealth = other.GetComponent<HealthManager>();
      if (targetHealth != null) {
        targetHealth.TakeDamage(damageAmount);
        DisableHitbox();
      }
    }
  }

  private void PlayImpactSound()
  {
    if (impactSound != null && audioSource != null)
    {
      // Randomize pitch for variation
      audioSource.pitch = Random.Range(0.8f, 1.2f);
      // PlayOneShot allows multiple sounds to overlap without cutting each other off
      audioSource.PlayOneShot(impactSound);
    }
  }
}