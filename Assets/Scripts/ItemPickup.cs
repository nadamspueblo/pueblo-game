using UnityEngine;
using StarterAssets;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData; // Drag your ScriptableObject data card here!

    [Header("World Space UI")]
    public GameObject promptCanvas; // Drag the local World Space Canvas here
    public TextMeshProUGUI promptText; // Drag the TMP object here

    private bool isPlayerInZone = false;
    private StarterAssetsInputs playerInputs;

    void Start()
    {
        // Hide the UI when the game starts
        if (promptCanvas != null) promptCanvas.SetActive(false);
        
        // Automatically set the text based on the ScriptableObject's name!
        if (promptText != null && itemData != null)
        {
            promptText.text = itemData.itemName;
        }
    }

    void Update()
    {
        if (isPlayerInZone && playerInputs != null && playerInputs.interact)
        {
            // Attempt to add the item to the Singleton inventory
            if (InventoryManager.Instance.AddItem(itemData))
            {
                // It successfully fit in the backpack! 
                // Hide the UI and destroy this 3D object from the world
                if (promptCanvas != null) promptCanvas.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                // The backpack returned false (it is full)
                if (promptText != null) promptText.text = "Full";
            }
            
            // Consume the input so they don't accidentally interact with something else
            playerInputs.interact = false; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInputs = other.transform.root.GetComponent<StarterAssetsInputs>();
            
            if (playerInputs != null)
            {
                isPlayerInZone = true;
                
                // Reset the text just in case it previously said "Inventory Full!"
                if (promptText != null && itemData != null) 
                {
                    promptText.text = itemData.itemName;
                }
                
                // Show the floating text
                if (promptCanvas != null) promptCanvas.SetActive(true); 
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerInputs = null; 
            
            // Hide the floating text when they walk away
            if (promptCanvas != null) promptCanvas.SetActive(false); 
        }
    }
}