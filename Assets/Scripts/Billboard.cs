using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Automatically find the player's camera when the game starts
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Force the canvas to look in the exact same direction the camera is looking.
            // This keeps the text perfectly readable from any angle!
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}