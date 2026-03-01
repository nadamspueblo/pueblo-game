# 🧟 Project: Campus Survival - Game Design Document (GDD)

## 📖 Game Overview
**Concept:** A 3rd-person zombie survival game set on our school campus. 
**The Hook:** The game begins in high-action: the player is sprinting into the school to escape a massive undead horde. 
**The Loop:** Once inside, the player must clear out zombies, unlock new zones of the school, gather resources, and establish/upgrade a fortified homebase. 

---

## 🏗️ Phase 1: The Core Loop (Minimum Viable Product)
*Rule: Do not move to Phase 2 until everything in Phase 1 is fully playable and bug-free.*

**1. Player Movement & Camera (✅ IN PROGRESS)**
* Utilize Unity Starter Assets Third-Person Controller.
* Ensure smooth Cinemachine camera orbiting and movement.

**2. Zombie AI & Pathfinding (✅ IN PROGRESS)**
* Implement Unity NavMesh for the campus floor.
* Zombies must idle, wander, and chase the player using `NavMeshAgent`.
* Zombies must avoid dynamic obstacles (desks, barricades) using `NavMeshObstacle` carving.

**3. Environment and Scenery (✅ IN PROGRESS)**
* Create texture assets for making materials and terrain layers
* Develop terrain
* Create building and structure prefabs
* Find/create scenery assets - trees, bushes, debris, desks, chairs, etc
* Build out scenes

**4. The Health & Stats System (✅ IN PROGRESS)**
* Create a universal `HealthManager` script attached to both the Player and Zombies.
* Track Max Health, Current Health, and handle a `TakeDamage()` function.
* **Player Death:** Triggers a "Game Over" screen.
* **Zombie Death:** Triggers a ragdoll physics effect or death animation, eventually destroys the zombie object.

**5. Basic Combat System (✅ IN PROGRESS)**
* Give the player a starting weapon (e.g., a baseball bat).
* Use Unity **Animation Events** to turn on a weapon "hitbox" only during the exact frames of the swing animation.
* If the hitbox collides with a zombie, trigger the zombie's `TakeDamage()` function.

---

## 🗺️ Phase 2: World Progression & Objectives
*Once we can fight and survive, we need a reason to explore.*

**1. The Zone Unlocking System (Story Progression)**
* Block off sections of the campus (Gym, Cafeteria, Science Wing) with locked doors or barricades.
* Place invisible "Trigger Colliders" near doors.
* Require the player to find specific items (keys, bolt cutters) or clear out a specific number of zombies to unlock the next zone.

**2. The Homebase System**
* Establish a safe zone (e.g., the Library or a specific classroom).
* Create interactive objects within the base that the player can upgrade using gathered resources (e.g., boarding up windows, building a workbench).

---

## 🎒 Phase 3: Systems & Economy
*Deepening the gameplay with items and crafting.*

**1. The Inventory System**
* Use **ScriptableObjects** to create a database of items (health kits, scrap metal, wood).
* Build a UI Canvas that updates to show what the player is currently carrying.
* Implement a system for picking up items from the 3D world and adding them to the UI list.

**2. The Crafting System**
* Create a "Workbench" UI.
* Write logic that checks the player's inventory for required recipes: `If (HasWood && HasNails) -> Create(SpikedBat)`.

---

## ✨ Phase 4: Polish & User Experience (UX)
*Wrapping the mechanics in a fully finished game package.*

**1. Main Menu & Game Flow**
* Create a visually appealing start screen (`00_MainMenu` scene).
* Include "Start Game", "Options", and "Quit" buttons.

**2. Character Selection**
* Allow the player to choose between 3-4 different student character models on the Main Menu.
* Pass that selection data into the main game scene to activate the correct 3D mesh on the player prefab.

**3. Pause Menu**
* Allow the player to press `Escape` or `P` to freeze the game (`Time.timeScale = 0`).
* Provide options to resume or return to the Main Menu.