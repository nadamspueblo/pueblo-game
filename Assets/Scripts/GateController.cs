using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
using TMPro;
public class GateController : MonoBehaviour
{
  [Header("Gate Components")]
  public Animator gateAnimator;
  public NavMeshObstacle gateObstacle;
  public float obstacleRemoveDelay = 1f;

  [Header("World Space UI")]
  public GameObject promptCanvas;
  public TextMeshProUGUI promptText;

  private bool isOpened = false;
  private bool inZone = false;
  private StarterAssetsInputs playerInputs;
  private float timer = 0f;

  void Start()
  {
    timer = 0f;
    // Make sure the floating text is completely hidden when the game starts
    if (promptCanvas != null) promptCanvas.SetActive(false);
  }

  void Update()
  {
    // If the player is standing in the zone, the gate isn't open yet, AND they press 'E'
    if (inZone && playerInputs != null && playerInputs.interact)
    {
      if (!isOpened)
      {
        OpenGate();
      }
      else
      {
        CloseGate();
      }

      UpdatePromptText(); // Switch the text from Open to Close

      // Hide the text the exact moment they push the gate
      if (promptCanvas != null) promptCanvas.SetActive(false);

      playerInputs.interact = false;
    }

    if (isOpened && !gateObstacle.enabled)
    {
      if (timer >= obstacleRemoveDelay)
        gateObstacle.enabled = false;
      else
        timer += Time.deltaTime;
    }
  }

  void OnTriggerEnter(Collider other)
  {
    // Check if the player stepped into the trigger, and make sure we haven't already opened it
    if (other.CompareTag("Player"))
    {
      playerInputs = other.transform.root.GetComponent<StarterAssetsInputs>();
      inZone = true;
      UpdatePromptText();

      // Show the floating text!
      if (promptCanvas != null) promptCanvas.SetActive(true);
    }
  }

  void OnTriggerExit(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      inZone = false;
      playerInputs = null;

      // Hide the floating text when they walk away
      if (promptCanvas != null) promptCanvas.SetActive(false);
    }
  }

  private void OpenGate()
  {
    if (gateAnimator != null)
    {
      gateAnimator.SetTrigger("OpenGate");
    }

    isOpened = true;
  }

  private void CloseGate()
  {
    if (gateAnimator != null)
    {
      gateAnimator.SetTrigger("CloseGate");
    }

    if (gateObstacle != null)
    {
      gateObstacle.enabled = true;
    }

    isOpened = false;
  }

  private void UpdatePromptText()
  {
    if (promptText != null)
    {
      promptText.text = !isOpened ? "Open (E)" : "Close (E)";
    }
  }
}