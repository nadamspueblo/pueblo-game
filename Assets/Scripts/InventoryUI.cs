using UnityEngine;
using TMPro;
using StarterAssets; // Needed to freeze the player

public class InventoryUI : MonoBehaviour
{
  [Header("UI References")]
  public CanvasGroup inventoryGroup;
  public TextMeshProUGUI itemNameText;
  public TextMeshProUGUI itemDescText;

  [Header("System References")]
  public Inventory3DPreview studio;
  public GameObject player;
  public ThirdPersonController playerController;
  public SurvivalStats  survivalStats;

  [Header("Input Keys")]
  public KeyCode toggleInventoryKey = KeyCode.Tab;
  public KeyCode cycleLeftKey = KeyCode.A;
  public KeyCode cycleRightKey = KeyCode.D;
  public KeyCode useKey = KeyCode.E;
  public KeyCode dropKey = KeyCode.X;

  private bool isInventoryOpen = false;
  private int currentIndex = 0;

  void Start()
  {
    HideInventory(); // Hide UI on start
  }

  void Update()
  {
    // 1. Toggle the Inventory Open/Closed
    if (Input.GetKeyDown(toggleInventoryKey))
    {
      ToggleInventory();
    }

    // 2. Handle Inputs ONLY if the inventory is open
    if (isInventoryOpen)
    {
      HandleCycling();
      HandleActions();
    }
  }

  void ToggleInventory()
  {
    isInventoryOpen = !isInventoryOpen;

    if (isInventoryOpen)
    {
      ShowInventory();

      // FREEZE THE PLAYER
      if (playerController != null) {
        playerController.enabled = false;
        playerController.ResetAnimationsToIdle();
      }

      // Check if backpack has items, then show the first one
      if (InventoryManager.Instance.items.Count > 0)
      {
        currentIndex = 0;
        UpdateDisplay();
      }
      else
      {
        ClearDisplay("Inventory Empty", "");
      }
    }
    else
    {
      HideInventory();

      // UNFREEZE THE PLAYER
      if (playerController != null) playerController.enabled = true;

      // Clear the 3D studio so an item isn't spinning underground while we play
      if (studio != null) studio.DisplayItem(null);
    }
  }

  void HandleCycling()
  {
    if (InventoryManager.Instance.items.Count <= 0) return;

    if (Input.GetKeyDown(cycleRightKey))
    {
      currentIndex++;
      if (currentIndex >= InventoryManager.Instance.items.Count) currentIndex = 0; // Wrap around
      UpdateDisplay();
    }
    else if (Input.GetKeyDown(cycleLeftKey))
    {
      currentIndex--;
      if (currentIndex < 0) currentIndex = InventoryManager.Instance.items.Count - 1; // Wrap around
      UpdateDisplay();
    }
  }

  void HandleActions()
  {
    if (InventoryManager.Instance.items.Count <= 0) return;

    ItemData currentItem = InventoryManager.Instance.items[currentIndex];

    if (Input.GetKeyDown(useKey))
    {
      Debug.Log("Used item: " + currentItem.itemName);
      // Restore the appropriate stat
      survivalStats.RestoreStat(currentItem.statToRestore, currentItem.restoreAmount);

      ConsumeCurrentItem();
    }
    else if (Input.GetKeyDown(dropKey))
    {
      Debug.Log("Dropped item: " + currentItem.itemName);
      // TODO: Instantiate the 3D prefab on the ground in front of the player later
      Vector3 location = new Vector3(player.transform.position.x + 0.3f, player.transform.position.y + 0.3f, player.transform.position.z + 0.3f);
      Instantiate(currentItem.itemPrefab, location, Quaternion.identity);

      ConsumeCurrentItem();
    }
  }

  void ConsumeCurrentItem()
  {
    // Remove from the actual backpack
    InventoryManager.Instance.RemoveItem(InventoryManager.Instance.items[currentIndex]);

    // Adjust the index so we don't go out of bounds if we just ate the last item
    if (currentIndex >= InventoryManager.Instance.items.Count)
    {
      currentIndex = InventoryManager.Instance.items.Count - 1;
    }

    // Redraw the screen
    if (InventoryManager.Instance.items.Count > 0)
    {
      UpdateDisplay();
    }
    else
    {
      ClearDisplay("Inventory Empty", "");
    }
  }

  void UpdateDisplay()
  {
    ItemData itemToDisplay = InventoryManager.Instance.items[currentIndex];

    // Update the text
    if (itemNameText != null) itemNameText.text = itemToDisplay.itemName;
    if (itemDescText != null) itemDescText.text = itemToDisplay.description;

    // Send the 3D model to the photo studio!
    if (studio != null) studio.DisplayItem(itemToDisplay);
  }

  void ClearDisplay(string title, string desc)
  {
    if (itemNameText != null) itemNameText.text = title;
    if (itemDescText != null) itemDescText.text = desc;
    if (studio != null) studio.DisplayItem(null);
  }

  void ShowInventory()
  {
    if (inventoryGroup != null)
    {
      inventoryGroup.alpha = 1f; // Make it 100% visible
      inventoryGroup.interactable = true; // Allow interactions
      inventoryGroup.blocksRaycasts = true; // Block mouse clicks from hitting the game world
    }
  }
  void HideInventory()
  {
    if (inventoryGroup != null)
    {
      inventoryGroup.alpha = 0f; // Make it 100% invisible
      inventoryGroup.interactable = false; // Disable interactions
      inventoryGroup.blocksRaycasts = false; // Let clicks pass through to the game
    }
  }
}