using UnityEngine;
using UnityEngine.AI;
using StarterAssets;

public class SlidingGate : MonoBehaviour
{
    [Header("Gate Movement")]
    public Transform gateTransform;
    public Vector3 slideDirection = Vector3.right; // Direction to slide (right, left, up, etc.)
    public float slideDistance = 3f; // How far the gate slides
    public float slideSpeed = 2f; // Speed of sliding animation
    
    [Header("Gate State")]
    public bool isOpen = false;
    public bool isMoving = false;
    
    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 3f;
    
    [Header("NavMesh Integration")]
    public NavMeshObstacle navObstacle;
    
    // Private variables
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Transform player;
    private bool playerInRange = false;
    
    void Start()
    {
        // Set up gate transform reference
        if (gateTransform == null)
            gateTransform = transform;
            
        // Calculate positions
        closedPosition = gateTransform.position;
        openPosition = closedPosition + (slideDirection.normalized * slideDistance);
        
        // Find player reference
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
            
        // Set up NavMesh obstacle
        if (navObstacle == null)
            navObstacle = GetComponent<NavMeshObstacle>();
    }
    
    void Update()
    {
        CheckPlayerInteraction();
    }
    
    void CheckPlayerInteraction()
    {
        if (player == null) return;
        
        // Check if player is in interaction range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= interactionRange;
        
        // Handle input when player is in range
        if (playerInRange && Input.GetKeyDown(interactionKey) && !isMoving)
        {
            ToggleGate();
        }
    }
    
    public void ToggleGate()
    {
        if (isMoving) return; // Prevent multiple activations
        
        if (isOpen)
        {
            CloseGate();
        }
        else
        {
            OpenGate();
        }
    }
    
    public void OpenGate()
    {
        if (isMoving || isOpen) return;
        
        StartCoroutine(SlideGate(openPosition, true));
    }
    
    public void CloseGate()
    {
        if (isMoving || !isOpen) return;
        
        StartCoroutine(SlideGate(closedPosition, false));
    }
    
    System.Collections.IEnumerator SlideGate(Vector3 targetPosition, bool opening)
    {
        isMoving = true;
        Vector3 startPosition = gateTransform.position;
        float elapsedTime = 0f;
        float duration = slideDistance / slideSpeed;
        
        // Disable NavMesh obstacle when opening
        if (opening && navObstacle != null)
        {
            navObstacle.enabled = false;
        }
        
        // Smoothly move the gate
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Use smooth curve for natural movement
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            gateTransform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            yield return null;
        }
        
        // Ensure final position is exact
        gateTransform.position = targetPosition;
        
        // Update state
        isOpen = opening;
        isMoving = false;
        
        // Re-enable NavMesh obstacle when closing
        if (!opening && navObstacle != null)
        {
            navObstacle.enabled = true;
        }
        
        Debug.Log($"Gate {(opening ? "opened" : "closed")}");
    }
    
    // Visual debugging in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw slide direction and distance
        Gizmos.color = Color.green;
        Vector3 start = (gateTransform != null) ? gateTransform.position : transform.position;
        Vector3 end = start + (slideDirection.normalized * slideDistance);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, Vector3.one * 0.5f);
    }
    
    // Optional: UI prompt display
    void OnGUI()
    {
        if (playerInRange && !isMoving)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            
            if (screenPos.z > 0) // Only show if in front of camera
            {
                string prompt = isOpen ? $"Close ({interactionKey})" : $"Open ({interactionKey})";
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 20), prompt);
            }
        }
    }
}
