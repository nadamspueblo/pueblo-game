# State of the Build: Survival Game Architecture

This document synthesizes the foundational backend architecture with the highly advanced cinematic logic and physics systems currently implemented. It serves as the complete blueprint for the current state of the project.

---

## 🧟‍♂️ 1. Enemy AI & Behaviors (`ZombieAdvancedAI`)
The zombie logic has evolved from basic pathfinding into a robust, physics-aware, cinematic threat.

* **State Machine Architecture:** A robust Finite State Machine handles complex behaviors, seamlessly transitioning between states like `Wander`, `Alert`, `Chase`, `QuickBite`, `Crawling`, `Unconscious`, and `Dead`.
* **Sensory Systems:** AI reactions are driven by both vision and a dynamic audio system (`NoiseMaker`). Zombies investigate sounds rather than relying purely on sight, enabling future stealth mechanics.
* **Cinematic Grapple Attacks (`QuickBite`):** Instead of relying on messy trigger hitboxes for grabs, the AI uses a mathematically calculated "Sticky Loop." The zombie suspends its `NavMeshAgent`, slides into a perfect 0.8-unit offset, and constantly recalculates its position and rotation to stay locked to the player's camera for the duration of the animation.
* **Crowd Control (Flocking):** Zombies utilize a lightweight `Physics.OverlapSphere` check to mathematically push each other away. This creates a terrifying, spreading horde line as they approach the player without causing physics engine conflicts, jittering, or nested Rigidbody teleportation bugs.
* **Root Motion Bypass:** The AI cleanly toggles `ignoreRootMotion` on the `RootMotionAnimation` script to prevent the Animator from fighting the cinematic positioning math during attacks.

## 🦴 2. Combat Architecture & Physics
The combat loop utilizes a highly modular system to handle locational damage and complex animation interactions.

* **The Relay Pattern:** Solves the *"AnimationEvent has no receiver!"* error by separating responsibilities so Animators only talk to scripts on the root object.
    * *Player Relay:* `PlayerMeleeAttack.cs` sits on the root player object. It receives Animation Events and passes the command down to the equipped weapon.
    * *Enemy Relay:* `ZombieCombat.cs` sits on the root enemy object and performs the exact same function for zombie attacks (like bites or claws).
* **Unified Hitboxes (`WeaponHitbox.cs`):** Sits on weapon prefabs (both player weapons and zombie attack colliders). It uses `OnTriggerEnter`, checks for self-hits, deals damage, handles randomized impact audio, and immediately disables its own collider to prevent multi-hit frame bugs.
* **Locational Damage (`ZombieBodyPart.cs`):** Enemies use individual colliders designated by a `PartType` enum (Head, Torso, Legs). Each part has local health and a specific damage multiplier to reward precision strikes. 
* **Damage Routing:** When a body part is hit, it triggers locational VFX/Animation via `DamageFeedback`. Main health (`HealthManager`) only takes damage after a specific body part is "broken" (local health reaches 0). The main enemy physics capsule is on the `EnemyMovement` layer, while individual locational colliders are strictly on the `EnemyHitbox` layer.
* **Ragdoll Physics (`RagdollManager`):**
    * *Recovery:* When knocked unconscious, zombies use a custom `LateUpdate` Slerp coroutine to smoothly blend their physics-driven bone transforms back into their animated standing frames without jarring snaps.
    * *Death & Optimization:* Upon health reaching zero (via the `onDeath` event), the zombie triggers its ragdoll, lies still for 5 seconds, and then executes a "Component Stripping" coroutine. It systematically deletes its joints, rigidbodies, colliders, and scripts, leaving behind a zero-cost static mesh corpse. 

## 🏃‍♂️ 3. The Player Controller (`ThirdPersonController`)
The player movement system handles heavy physical interactions, weapon commitment, and two-way communication with the enemy AI.

* **Animation & Layering:** Melee attack animations are placed on the Base Layer (not an Upper Body mask) to allow full-body commitment. This prevents foot-sliding and makes the "melee magnetism" (lunging toward targets) feel impactful.
* **Combat Mode:** The player can toggle a combat state that locks their rotation to the main camera, using a smooth dampener for realistic turning.
* **The Grapple "Tollbooth":** When a zombie initiates a grab, it passes its own reference to the player. The player's movement isn't stopped, but heavily penalized—speed drops to 15%, and `GrappledRotationSmoothTime` is increased to make turning the camera feel like dragging dead weight.
* **QTE Escape Mechanic:** While grappled, the player can press an escape key (E). The player script tells the specific grabbing zombie to run `BreakGrapple()`, which safely interrupts the bite coroutine, plays a stagger animation on the zombie, and restores the player's movement instantly.

## 🎒 4. Inventory, Items & Environment
A modular, scalable system for handling loot and equipping gear, designed to work smoothly with ProBuilder blockouts and downloaded asset packs.

* **Data Structures (`ItemData.cs`):** Uses `ScriptableObjects` to define items. Boolean flags were replaced with an `ItemType` enum (Consumable, CraftingMaterial, Weapon, Equipment, Misc) for clean switch statement logic in the UI.
* **Weapon Equipping (`InventoryUI.cs`):** Weapons are instantiated as children of a cached `WeaponHolder` transform. We use `Instantiate(prefab, parent, false)` to ensure the weapon respects the local position and rotation offsets baked into the prefab using the Inspector's "Overrides -> Apply All" workflow.
* **3D Inventory Preview (`Inventory3DPreview.cs`):** * *Dynamic Scaling:* Calculates the total `Renderer.bounds` of the instantiated prefab, finds the maximum dimension, and mathematically scales it to fit a specific `targetSize` in the UI camera.
    * *Centering & Rotation:* Calculates the true visual center of the bounds to align the item perfectly with the spawn point, using `RotateAround()` to prevent off-center pivot wobble.
    * *Safety:* Strips all `Collider` components from the spawned preview object and its children so UI weapons don't accidentally trigger physics or damage events.

---

## 🗺️ 5. The Roadmap (Next Up)
* **Player Stealth:** Integrating a crouching state into the `ThirdPersonController` that modifies movement speed and animation.
* **Audio Detection Integration:** Hooking the player's movement states (walking, sprinting, crouching) into the dynamic `NoiseMaker` radius so the player can actively utilize the environment to sneak past the horde.