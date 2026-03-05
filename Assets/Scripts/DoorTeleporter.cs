using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [Header("Teleportation Settings")]
    [Tooltip("The transform where the player will be teleported to")]
    public Transform teleportDestination;
    
    [Header("Interaction Settings")]
    [Tooltip("Distance the player needs to be within to interact")]
    public float interactionRange = 3f;
    
    [Tooltip("Key to press for interaction (default is E)")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Feedback")]
    [Tooltip("Text to show when player can interact")]
    public string interactionPrompt = "Press E to open door";
    
    // Private variables
    private GameObject player;
    private bool playerInRange = false;
    private CharacterController playerController;
    
    void Start()
    {
        // Find the player object (assuming it has the "Player" tag)
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Get the CharacterController component for teleportation
            playerController = player.GetComponent<CharacterController>();
        }
        else
        {
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
        }
    }
    
    void Update()
    {
        // Check if player is in range
        CheckPlayerDistance();
        
        // Handle interaction input
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            TeleportPlayer();
        }
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // Update playerInRange based on distance
        playerInRange = distanceToPlayer <= interactionRange;
    }
    
    void TeleportPlayer()
    {
        if (teleportDestination == null)
        {
            Debug.LogError("No teleport destination set!");
            return;
        }
        
        if (playerController != null)
        {
            // Disable the CharacterController temporarily to avoid conflicts
            playerController.enabled = false;
            
            // Move the player to the destination
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;
            
            // Re-enable the CharacterController
            playerController.enabled = true;
            
            Debug.Log("Player teleported to: " + teleportDestination.name);
        }
        else
        {
            // Fallback if no CharacterController (shouldn't happen with your setup)
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;
        }
    }
    
    // Visual feedback in the Scene view
    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to destination if set
        if (teleportDestination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, teleportDestination.position);
            Gizmos.DrawWireCube(teleportDestination.position, Vector3.one);
        }
    }
}
