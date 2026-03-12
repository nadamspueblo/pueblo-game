using UnityEngine;
using System.Collections.Generic;
using StarterAssets; 

public class DoorBarricade : MonoBehaviour
{
    [Header("Barricade Settings")]
    public List<GameObject> boards = new List<GameObject>(); // Drag your 2x4 board objects here
    public float interactionDistance = 2f;
    public float removalTime = 1.5f; // Time to rip off each board
    
    [Header("UI")]
    public GameObject interactionPrompt; // UI element showing "Press E to remove board"
    
    private Transform player;
    private bool isRemoving = false;
    private int currentBoardIndex = 0;
    
    void Start()
    {
        // Find the player - assuming you're using the PlayerArmature prefab
        player = FindObjectOfType<ThirdPersonController>()?.transform;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
         
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    
        if (interactionPrompt != null)
        interactionPrompt.SetActive(false); 
    }
    
    void Update()
    {
        // Check if player is close enough and there are boards left
        if (player != null && boards.Count > currentBoardIndex)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= interactionDistance && !isRemoving)
            {
                // Show interaction prompt
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
                
                // Check for input (assuming standard input system)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(RemoveBoard());
                }
            }
            else
            {
                // Hide interaction prompt
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);
            }
        }
    }
    
    System.Collections.IEnumerator RemoveBoard()
    {
        if (currentBoardIndex >= boards.Count) yield break;
        
        isRemoving = true;
        
        // Hide interaction prompt during removal
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // Get the current board to remove
        GameObject boardToRemove = boards[currentBoardIndex];
        
        // Add some visual feedback - shake the board
        Vector3 originalPosition = boardToRemove.transform.localPosition;
        float shakeIntensity = 0.01f;
        float elapsed = 0f;
        
        while (elapsed < removalTime)
        {
            // Shake effect
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            boardToRemove.transform.localPosition = originalPosition + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset position then "rip off" the board
        boardToRemove.transform.localPosition = originalPosition;
        
        // Add physics to make it fall/fly off
        Rigidbody rb = boardToRemove.GetComponent<Rigidbody>();
        if (rb == null)
            rb = boardToRemove.AddComponent<Rigidbody>();
        
        // Apply force to make it look like it was ripped off
        Vector3 forceDirection = (boardToRemove.transform.position - player.position).normalized;
        forceDirection += Vector3.up * 0.5f; // Add some upward force
        rb.AddForce(forceDirection * -5f, ForceMode.Impulse);
        
        // Move to next board
        currentBoardIndex++;
        
        // If all boards are removed, you might want to trigger something
        if (currentBoardIndex >= boards.Count)
        {
            OnAllBoardsRemoved();
        }
        
        isRemoving = false;
    }
    
    void OnAllBoardsRemoved()
    {
        // This is where you can add logic for what happens when the doorway is clear
        Debug.Log("Doorway cleared! Player can now pass through.");
        
        // Maybe enable a door script, update NavMesh, etc.
        // You could also trigger a sound effect or particle effect here
    }
    
    // Visual helper in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
