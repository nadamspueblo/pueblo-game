using UnityEngine;

public class ConnectExistingChain : MonoBehaviour
{
    [Header("Chain Connection Settings")]
    public bool anchorTopLink = true;
    public float jointBreakForce = 1000f;
    
    void Start()
    {
        // Stop all movement first
        FreezeAllLinks();
        
        // Wait a tiny bit, then connect
        Invoke("ConnectChainLinks", 0.1f);
    }
    
    void FreezeAllLinks()
    {
        // Temporarily freeze all rigidbodies
        for (int i = 0; i < transform.childCount; i++)
        {
            Rigidbody rb = transform.GetChild(i).GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Temporarily freeze
            }
        }
    }
    
    void ConnectChainLinks()
    {
        // Get all chainlink objects as children
        Transform[] chainLinks = new Transform[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            chainLinks[i] = transform.GetChild(i);
        }
        
        // Sort by Y position (top to bottom)
        System.Array.Sort(chainLinks, (a, b) => b.position.y.CompareTo(a.position.y));
        
        // Connect each link
        for (int i = 0; i < chainLinks.Length; i++)
        {
            GameObject currentLink = chainLinks[i].gameObject;
            Rigidbody currentRb = currentLink.GetComponent<Rigidbody>();
            
            // Connect to previous link
            if (i > 0)
            {
                GameObject previousLink = chainLinks[i - 1].gameObject;
                Rigidbody previousRb = previousLink.GetComponent<Rigidbody>();
                
                HingeJoint joint = currentLink.AddComponent<HingeJoint>();
                joint.connectedBody = previousRb;
                joint.axis = Vector3.forward;
                joint.anchor = Vector3.up * 0.5f;
                joint.breakForce = jointBreakForce;
            }
            
            // Unfreeze all links except the top one
            if (i == 0 && anchorTopLink)
            {
                currentRb.isKinematic = true; // Keep top anchored
            }
            else
            {
                currentRb.isKinematic = false; // Allow physics
            }
        }
        
        Debug.Log("Chain connected smoothly!");
    }
}
