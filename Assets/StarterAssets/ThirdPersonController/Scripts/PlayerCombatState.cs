using UnityEngine;
using StarterAssets;

public class PlayerCombatState : MonoBehaviour
{
    [Header("References")]
    public ThirdPersonController tpc;
    
    [Header("Input")]
    public KeyCode aimKey = KeyCode.Mouse1; // Right Click
    
    void Start()
    {
        if (tpc == null) tpc = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (tpc != null)
        {
            // If holding right-click, tell the movement script to enter combat mode!
            if (Input.GetKey(aimKey))
            {
                tpc.isCombatMode = true;
            }
            else
            {
                tpc.isCombatMode = false;
            }
        }
    }
}