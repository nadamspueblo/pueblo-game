using UnityEngine;

public enum ItemType 
{ 
    Consumable, 
    CraftingMaterial, 
    Weapon, 
    Equipment, 
    Misc 
}

public enum WeaponType
{
  Unarmed,
  Melee1Hand,
  Melee2Hand
}

[CreateAssetMenu(fileName = "New Item", menuName = "Survival Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon; 

    [Header("Item Type")]
    public ItemType type; // Dropdown in the Inspector!

    [Header("Consumable Stats")]
    [Tooltip("health, hunger, thirst, sleep")]
    public string statToRestore; 
    public float restoreAmount;

    [Header("Weapon Stats")]
    public WeaponType weaponType;
    public float damage;
    public float attackRange;

    [Header("3D Representation")]
    public GameObject itemPrefab;
}