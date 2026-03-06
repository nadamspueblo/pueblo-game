using UnityEngine;
using System.Collections;

public class WeaponHolster : MonoBehaviour
{
    [Header("Weapon Positions")]
    public Transform upperLegHolster;     // UpperLegHolster you created
    
    [Header("Current Weapon")]
    public GameObject currentWeapon;      // Your weapon
    
    [Header("Settings")]
    public float transitionSpeed = 8f;    // Animation speed
    public bool allowHolsterWhileMoving = true;
    
    // The script will remember these positions
    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    
    private bool weaponDrawn = true;
    private bool isMoving = false;
    
    // NEW: Animator reference
    private Animator playerAnimator;
    
    void Start()
    {
        // Remember where the weapon starts
        if (currentWeapon != null)
        {
            originalParent = currentWeapon.transform.parent;
            originalLocalPos = currentWeapon.transform.localPosition;
            originalLocalRot = currentWeapon.transform.localRotation;
            
            Debug.Log("Remembered weapon position: " + originalLocalPos);
        }
        
        // NEW: Get animator reference
        playerAnimator = GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogWarning("No Animator found on player! Attack blocking won't work.");
        }
        else
        {
            // Set initial state - weapon starts drawn
            playerAnimator.SetBool("WeaponDrawn", weaponDrawn);
            Debug.Log("Set WeaponDrawn parameter to: " + weaponDrawn);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isMoving)
        {
            ToggleWeapon();
        }
    }
    
    void ToggleWeapon()
    {
        if (currentWeapon == null || upperLegHolster == null || originalParent == null)
        {
            Debug.LogError("Missing required components for holstering!");
            return;
        }
        
        if (weaponDrawn)
        {
            StartCoroutine(MoveWeaponToHolster());
            weaponDrawn = false;
            Debug.Log("Holstering weapon...");
        }
        else
        {
            StartCoroutine(MoveWeaponToHand());
            weaponDrawn = true;
            Debug.Log("Drawing weapon...");
        }
        
        // NEW: Update animator parameter immediately
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("WeaponDrawn", weaponDrawn);
            Debug.Log("Updated WeaponDrawn parameter to: " + weaponDrawn);
        }
    }
    
    // ... (keep all your existing MoveWeaponToHolster, MoveWeaponToHand, and other methods the same) ...
    
    IEnumerator MoveWeaponToHolster()
    {
        isMoving = true;
        
        Vector3 startLocalPos = currentWeapon.transform.localPosition;
        Quaternion startLocalRot = currentWeapon.transform.localRotation;
        Transform startParent = currentWeapon.transform.parent;
        
        currentWeapon.transform.SetParent(upperLegHolster);
        
        Vector3 targetLocalPos = Vector3.zero;
        Quaternion targetLocalRot = Quaternion.identity;
        
        float moveTime = 1f / transitionSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / moveTime);
            float easedProgress = EaseInOutQuart(progress);
            
            Vector3 worldStartPos = startParent.TransformPoint(startLocalPos);
            Quaternion worldStartRot = startParent.rotation * startLocalRot;
            
            Vector3 worldTargetPos = upperLegHolster.TransformPoint(targetLocalPos);
            Quaternion worldTargetRot = upperLegHolster.rotation * targetLocalRot;
            
            Vector3 currentWorldPos = Vector3.Lerp(worldStartPos, worldTargetPos, easedProgress);
            Quaternion currentWorldRot = Quaternion.Lerp(worldStartRot, worldTargetRot, easedProgress);
            
            currentWeapon.transform.position = currentWorldPos;
            currentWeapon.transform.rotation = currentWorldRot;
            
            yield return null;
        }
        
        currentWeapon.transform.localPosition = targetLocalPos;
        currentWeapon.transform.localRotation = targetLocalRot;
        
        SetWeaponActive(false);
        isMoving = false;
        Debug.Log("Weapon holstered!");
    }
    
    IEnumerator MoveWeaponToHand()
    {
        isMoving = true;
        
        Vector3 startLocalPos = currentWeapon.transform.localPosition;
        Quaternion startLocalRot = currentWeapon.transform.localRotation;
        Transform startParent = currentWeapon.transform.parent;
        
        currentWeapon.transform.SetParent(originalParent);
        
        Vector3 targetLocalPos = originalLocalPos;
        Quaternion targetLocalRot = originalLocalRot;
        
        float moveTime = 1f / transitionSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / moveTime);
            float easedProgress = EaseInOutQuart(progress);
            
            Vector3 worldStartPos = startParent.TransformPoint(startLocalPos);
            Quaternion worldStartRot = startParent.rotation * startLocalRot;
            
            Vector3 worldTargetPos = originalParent.TransformPoint(targetLocalPos);
            Quaternion worldTargetRot = originalParent.rotation * targetLocalRot;
            
            Vector3 currentWorldPos = Vector3.Lerp(worldStartPos, worldTargetPos, easedProgress);
            Quaternion currentWorldRot = Quaternion.Lerp(worldStartRot, worldTargetRot, easedProgress);
            
            currentWeapon.transform.position = currentWorldPos;
            currentWeapon.transform.rotation = currentWorldRot;
            
            yield return null;
        }
        
        currentWeapon.transform.localPosition = targetLocalPos;
        currentWeapon.transform.localRotation = targetLocalRot;
        
        SetWeaponActive(true);
        isMoving = false;
        Debug.Log("Weapon ready!");
    }
    
    float EaseInOutQuart(float t)
    {
        return t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;
    }
    
    void SetWeaponActive(bool active)
    {
        Collider weaponCollider = currentWeapon.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = active;
        }
        
        MonoBehaviour[] scripts = currentWeapon.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script.GetType() != typeof(WeaponHolster))
            {
                script.enabled = active;
            }
        }
    }
    
    public bool IsWeaponDrawn()
    {
        return weaponDrawn;
    }
}
