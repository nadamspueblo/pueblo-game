using UnityEngine;

public class Inventory3DPreview : MonoBehaviour
{
    [Header("Studio Setup")]
    public Transform spawnPoint; // Drag your ItemSpawnPoint here
    public float rotationSpeed = 30f; // Let's make the item slowly spin!
    
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
            // Spawn it at the exact position and rotation of our spawn point
            currentSpawnedModel = Instantiate(itemToDisplay.itemPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Optional: Strip away any physics scripts (like ItemPickup or Colliders) 
            // so the item just sits there and looks pretty!
            Destroy(currentSpawnedModel.GetComponent<ItemPickup>());
            Destroy(currentSpawnedModel.GetComponent<Collider>());
        }
    }

    void Update()
    {
        // 3. Slowly spin the item like a showcase!
        if (currentSpawnedModel != null)
        {
            currentSpawnedModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}