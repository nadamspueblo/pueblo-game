using UnityEngine;

// This line adds a new option to Unity's right-click menu
[CreateAssetMenu(fileName = "New Item", menuName = "Survival Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon; // For the UI later

    [Header("Item Type Flags")]
    public bool isConsumable;
    public bool isCraftingMaterial;

    [Header("Consumable Stats")]
    // Which stat does it fill? "health", "hunger", "thirst", "sleep"
    [Tooltip("health, hunger, thirst, sleep")]
    public string statToRestore; 
    public float restoreAmount;
}