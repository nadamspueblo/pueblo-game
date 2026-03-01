# 🧟 Game Dev Club: Custom NPCs & Smart Obstacles

This guide will show you how to bring your downloaded Mixamo characters (like zombies or other NPCs) to life, stop them from "ice-skating" or moonwalking, and make sure they don't walk through our furniture!

## Part 1: Importing & Setting Up a Mixamo NPC

### 🛑 Step 1: The Mixamo Download Rules
* Find models and animations at [https://www.mixamo.com/](https://www.mixamo.com/)
* **The Character:** Select a character then add animations and download your character as **FBX for Unity**.
* **The Animations:** When downloading animations separately (Idle, Walk, Run), change the Skin setting to **Without Skin**.
* **CRITICAL:** For moving animations (Walk, Chase, Run), ensure the **In Place** box is **UNCHECKED** before downloading. The animation needs actual forward movement to work correctly in our game!

### 🎨 Step 2: Fixing the Materials (The "Gray Zombie" Fix)
When you first drag your character into Unity, it might look gray or neon pink. Let's fix that.
1. Click your character's `.fbx` file in the Project window.
2. In the Inspector, go to the **Materials** tab.
3. Click **Extract Textures...** and save them in a new `Textures` folder.
4. Click **Extract Materials...** and save them in a new `Materials` folder.
5. If your character is bright pink, select all the extracted material spheres, go to the top menu, and click **Edit > Rendering > Materials > Convert Selected Built-in Materials to URP**.

### 🦴 Step 3: The Humanoid Rig
Unity needs to know your character has a human skeleton.
1. Select your character's `.fbx` file *and* all your animation `.fbx` files.
2. Go to the **Rig** tab in the Inspector.
3. Change **Animation Type** to **Humanoid** and click **Apply**. 

### 🎬 Step 4: Fixing the Animations for Root Motion
We need to make sure the animations loop and drive the character forward without drifting.
1. Select your animation `.fbx` files.
2. Go to the **Animation** tab in the Inspector.
3. Check **Loop Time**.
4. **Root Transform Rotation:** Check **Bake Into Pose** (keeps them from drifting sideways).
5. **Root Transform Position (Y):** Check **Bake Into Pose** and set Based Upon to **Feet** (keeps them locked to the floor).
6. **Root Transform Position (XZ):** **UNCHECK Bake Into Pose** (this allows the animation to physically pull the character forward). Click **Apply**.
7. **Extract the Clips:** Click the little arrow next to your animation `.fbx` to open it. Click the triangle named `mixamo.com` and press **Ctrl + D** (or Cmd + D) to duplicate it. Rename this new file something smart (like `Zombie_Walk`). Use these extracted clips for your Animator!

### 🧠 Step 5: The Brains (Animator & Scripts)
1. Right-click your Project window and create an **Animator Controller**. Name it `ZombieAnim`.
2. Open it, go to the Parameters tab, and add a Float named **VelocityZ** (Case sensitive!).
3. Right-click the grid, select **Create State > From New Blend Tree**. Name it `Locomotion`.
4. Double-click the Blend Tree. Change Blend Type to **1D** and set the Parameter to `VelocityZ`. Add your Idle, Walk, and Run animations and space out their thresholds (0, 1.5, 3.5). 
5. **Turn on Foot IK:** Click each motion field inside your Blend Tree and check the **Foot IK** box in the Inspector. Then, click the Gear icon next to the "Base Layer" in the top left and check **IK Pass**.
6. Drag your Character model into the Scene.
7. Give it your new `ZombieAnim` controller. **Check Apply Root Motion** so the animation drives the movement.
8. Add the **StudentNPCTemplate** script. This automatically adds a `NavMeshAgent` component. Adjust the radius/height to fit your character, set its speed, and drag the Player into the "Target To Chase" slot on the script!
9. **Fix Hovering:** Look at the `NavMeshAgent` component. If the zombie is floating, slightly lower the **Base Offset** (e.g., `-0.05`) until the shoes touch the blue NavMesh floor.
10. Add the **RootMotionAnimation** script. This allows the animator to vary the speed so that it lines up with the animation.
---

## Part 2: Smart Furniture (NavMesh Obstacles)

If an object never moves (like a wall), we bake it into the floor. If an object *can* be moved by the player or physics (like a chair, desk, or trash can), we use a `NavMesh Obstacle` so the AI knows to walk around it.

### 🛑 The Golden Rule: Only do this on PREFABS!
1. Go to your `Assets/_Project/Prefabs/` folder.
2. Double-click your furniture Prefab to open it in **Prefab Mode**.
3. In the Inspector, click **Add Component** and search for **NavMesh Obstacle**.
4. Change the Shape to match your object (Box or Capsule) and adjust the Size so the green wireframe surrounds it.
5. **Check the CARVE box!** This is the most important step. It tells the floor to dynamically cut a hole under the desk so the zombie has to walk around it. 
6. Keep **Carve Only Stationary** checked.
7. Save your Prefab and exit back to the Scene!