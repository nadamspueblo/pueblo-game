using UnityEngine;
using System.Collections.Generic;

public class HordeManager : MonoBehaviour
{
    // A quick way for any zombie to find this script instantly
    public static HordeManager Instance; 

    [Header("Slot Configuration")]
    [Tooltip("The center point the horde is circling (usually the Player)")]
    public Transform target; 
    
    [Tooltip("How many zombies can circle the player at once?")]
    public int maxSlots = 8;
    
    [Tooltip("How far away should they stand when circling?")]
    public float slotRadius = 2.5f;

    // The array holding our slots. If slots[2] has a zombie in it, that slot is taken!
    private ZombieAdvancedAI[] slots;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(this);

        slots = new ZombieAdvancedAI[maxSlots];
        
        // Auto-assign the target to the player if left blank
        if (target == null) target = this.transform; 
    }

    // Zombies call this to get a flanking position
    public int RequestSlot(ZombieAdvancedAI zombie)
    {
        // 1. Are they already assigned a slot? Keep it!
        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i] == zombie) return i;
        }

        // 2. Find the closest OPEN slot to the zombie's current position
        int bestSlot = -1;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i] == null) // The slot is empty!
            {
                Vector3 slotPos = GetPositionForSlot(i);
                float dist = Vector3.Distance(zombie.transform.position, slotPos);
                
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestSlot = i;
                }
            }
        }

        // 3. Claim the slot and return the ID
        if (bestSlot != -1)
        {
            slots[bestSlot] = zombie;
        }
        
        return bestSlot; // Returns -1 if the player is fully surrounded (no slots left)
    }

    // Zombies call this when they die, get knocked out, or wander away
    public void ReleaseSlot(ZombieAdvancedAI zombie)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i] == zombie)
            {
                slots[i] = null; // Free up the slot for another zombie
                return;
            }
        }
    }

    // Calculates the exact XYZ world coordinates for a specific slot
    public Vector3 GetPositionForSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots || target == null) return Vector3.zero;

        // Divide 360 degrees by our max slots (e.g., 360 / 8 = 45 degrees per slot)
        float angleStep = 360f / maxSlots;
        float angle = slotIndex * angleStep;

        // Calculate the direction using world rotation so the slots don't spin wildly when the player turns their camera
        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        
        return target.position + (direction * slotRadius);
    }

    // Draws helpful rings in the Scene view so you can visually tune the slotRadius!
    private void OnDrawGizmos()
    {
        if (target == null || slots == null|| !Application.isPlaying) return;
        for (int i = 0; i < maxSlots; i++)
        {
            // Red if taken, Green if empty
            Gizmos.color = (slots != null && slots.Length > i && slots[i] != null) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(GetPositionForSlot(i), 0.3f);
        }
    }
}