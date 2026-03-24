using UnityEngine;

public class Inventory3DPreview : MonoBehaviour
{
  [Header("Studio Setup")]
  public Transform spawnPoint; // Drag your ItemSpawnPoint here
  public float rotationSpeed = 30f; // Let's make the item slowly spin!

  [Header("Auto-Scaling")]
  [Tooltip("The ideal uniform size for items in the camera view")]
  public float targetSize = 2f;

  private GameObject currentSpawnedModel;

  // We will call this function from our UI buttons later!
  public void DisplayItem(ItemData itemToDisplay)
  {
    // 1. Clear out the old item
    if (currentSpawnedModel != null)
    {
      Destroy(currentSpawnedModel);
    }

    // 2. Spawn the new item if it has a 3D prefab!
    if (itemToDisplay != null && itemToDisplay.itemPrefab != null)
    {
      // Spawn it as a child of the spawn point
      currentSpawnedModel = Instantiate(itemToDisplay.itemPrefab, spawnPoint);

      // Reset local transform to zero out any weird prefab saves
      currentSpawnedModel.transform.localPosition = Vector3.zero;
      currentSpawnedModel.transform.localRotation = Quaternion.identity;

      // 3. Strip away physics scripts
      if (currentSpawnedModel.GetComponent<ItemPickup>())
        Destroy(currentSpawnedModel.GetComponent<ItemPickup>());

      // Weapons often have colliders on child objects (like your WeaponHitbox), 
      // so we need to destroy ALL colliders, not just the one on the root.
      Collider[] colliders = currentSpawnedModel.GetComponentsInChildren<Collider>();
      foreach (Collider col in colliders)
      {
        Destroy(col);
      }

      // 4. Run our new magical math function
      FitAndCenterItem();
    }
  }

  private void FitAndCenterItem()
  {
    // Get all visible meshes in the spawned model
    Renderer[] renderers = currentSpawnedModel.GetComponentsInChildren<Renderer>();

    if (renderers.Length == 0) return; // If there's no mesh, exit early

    // Create a starting bounding box using the first mesh
    Bounds bounds = renderers[0].bounds;

    // Expand the box to include every other mesh in the prefab
    for (int i = 1; i < renderers.Length; i++)
    {
      bounds.Encapsulate(renderers[i].bounds);
    }

    // Find the longest side of the bounding box
    float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

    // Prevent division by zero errors
    if (maxDimension > 0.001f)
    {
      // Calculate how much we need to scale the object to match our target size
      float scaleFactor = targetSize / maxDimension;

      // Apply the scale
      currentSpawnedModel.transform.localScale = Vector3.one * scaleFactor;

      // Re-calculate the bounding box now that the object has shrunk/grown
      bounds = renderers[0].bounds;
      for (int i = 1; i < renderers.Length; i++)
      {
        bounds.Encapsulate(renderers[i].bounds);
      }

      // Find the distance between the prefab's visual center and our ideal spawn point
      Vector3 offset = spawnPoint.position - bounds.center;

      // Nudge the model so its true visual mass is perfectly centered on the spawn point
      currentSpawnedModel.transform.position += offset;
    }
  }

  void Update()
  {
    // 5. Slowly spin the item like a showcase!
    if (currentSpawnedModel != null)
    {
      // Important fix: Because we nudged the object away from its pivot point to center it, 
      // normal Rotate() would cause it to wobble like a top. 
      // RotateAround forces it to orbit the exact center of our spawn point smoothly!
      currentSpawnedModel.transform.RotateAround(spawnPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
  }
}