using System.Collections.Generic; // Required to use Lists!
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int maxSlots = 10;
    
    // The actual backpack! It holds our ScriptableObject data cards.
    public List<ItemData> items = new List<ItemData>();

    [Header("Events")]
    // We will use this to tell the UI grid to redraw itself later
    public UnityEvent onInventoryChanged; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Other scripts call this when the player interacts with a 3D item on the ground
    public bool AddItem(ItemData itemToAdd)
    {
        if (items.Count < maxSlots)
        {
            items.Add(itemToAdd);
            Debug.Log("Picked up: " + itemToAdd.itemName);
            
            onInventoryChanged?.Invoke();
            return true; // Successfully picked up
        }
        
        Debug.Log("Inventory is full!");
        return false; // Backpack is full, leave it on the ground
    }

    // We will call this when the player clicks an item in their UI to eat/drink it
    public void RemoveItem(ItemData itemToRemove)
    {
        if (items.Contains(itemToRemove))
        {
            items.Remove(itemToRemove);
            onInventoryChanged?.Invoke();
        }
    }
}