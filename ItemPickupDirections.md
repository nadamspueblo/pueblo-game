# 🎒 How to Create a Pick-Up Item in Unity
In our survival game, items are broken into two parts: the **Data** (what the item is) and the **Physical Object** (the 3D model you see on the ground). This guide shows you how to connect them so the player can pick them up and add them to their inventory.

*Prerequisite*: Make sure your Player character has the InventoryManager script attached to it before you begin!

## Step 1: Create the Item Data (The Blueprint)
Before we make the 3D model, we need to create a data card that tells the game what the item actually is.
1. Go to your Project window at the bottom of the screen.
3. Right-click in an empty space and select Create > Survival Game > Item Data.
4. Name the new file something clear, like WaterBottle_Data.
5. Click on your new file and look at the Inspector.
6. Fill in the Item Name (e.g., "Water Bottle").
7. Check the Is Consumable box.
8. Type exactly which stat it restores ("thirst", "hunger", "health", "sleep") and set the Restore Amount (e.g., 25).

## Step 2: Build the Physical Object
Now we need the 3D model that the player will actually walk up to in the game world.
1. Drag your 3D model (or a simple Unity Cube/Cylinder) into your scene.
3. Select the object and look at the Inspector.
4. Click Add Component and search for Box Collider (or Sphere Collider).
5. Check the box labeled Is Trigger. This allows the player to walk through the interaction zone without bumping into an invisible wall.
6. Click the Edit Bounding Volume button (the icon with tiny squares on the collider) and scale the green box up slightly so the player doesn't have to stand exactly on top of the item to grab it.
7. Click Add Component again and attach the ItemPickup script.
8. Drag your WaterBottle_Data file from the Project window into the empty Item Data slot on the script.

## Step 3: Add the Floating Hologram Prompt
We need a 3D UI prompt to tell the player to press 'E'. We will make a tiny canvas that hovers over the item.

1. Right-click your 3D item in the Hierarchy and select UI > Canvas.
2. Select the new Canvas and change its Render Mode to World Space.
3. In the Rect Transform, set Pos X, Y, and Z to 0.
4. Change the Width to 2 and Height to 1.
5. Change the Scale (X, Y, and Z) to 0.01.
6. Set the Pivot to X: 0.5 and Y: 0.5.
7. Use the Move Tool (W) to slide the Canvas slightly above the item.
8. Click Add Component and attach the Billboard script so the canvas always rotates to face the player.
9. Right-click the Canvas and select UI > Text - TextMeshPro.
10. Center the text alignment and check the Auto Size box.

## Step 4: Wire It Together and Test
Finally, we just need to link the UI to the script so it can turn on and off.

1. Select your parent 3D item again.
2. Drag your new floating Canvas into the script's Prompt Canvas slot.
3. Drag the Text (TMP) object into the script's Prompt Text slot.
4. Hit Play!

Walk your character into the trigger box. The prompt should automatically read your data card and say "Press E to pick up Water Bottle". Press E, the object will disappear, and if you check your Unity Console, you will see a message confirming it was added to your backpack!