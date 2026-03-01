# 🏫 Game Dev Club: Unity & GitHub Workflow

Welcome to the project! Because we are all building this school campus together, we have to follow specific rules so we don't accidentally overwrite each other's hard work. 

We use a **Branching and Pull Request** workflow. This means nobody edits the `main` version of the game directly. Instead, you create a safe copy (a branch), make your changes there, and then ask for it to be merged in.

## 🛑 Step 1: PULL and BRANCH Before You Do Anything
Before you open Unity, you must get the latest updates and create your own safe workspace.
1. Open the **Source Control** tab in VS Code
2. Switch to the `main` branch.
3. **PULL (or Fetch/Pull Origin)** to make sure your `main` branch is 100% up to date.
4. **Create a New Branch.** Name it something clear using this format: `yourname/feature-name` (Example: `alex/cafeteria-tables` or `sam/gym-lighting`).
5. **Publish/Push** your new branch to the remote repository.
6. *Now* you can open the Unity project.

## 🛠️ Step 2: Working on the Game (The Scene Rules)
We do not have one giant "Main" scene. The school is broken up into smaller pieces so multiple people can work at the same time. 

**How to open your workspace:**
1. In the Project window, go to `Assets/Scenes/` and open `MainScene`. (This has our GameManager and lighting).
2. Go to `Assets/Scenes/` and find your assigned area (e.g., `CafeteriaScene`, `GymScene`, `ScienceWingScene`).
3. **Right-click** your assigned scene and select **Open Scene Additively**.
4. In the Hierarchy, right-click your assigned scene (e.g., `CafeteriaScene`) and select **Set Active Scene**. *(This ensures new things you create go into your scene, not the Main scene!)*

**The Golden Rule:** Only save changes to YOUR assigned scene. Never save changes to other scenes unless you were specifically asked to.

## 📦 Step 3: PREFABS Are Your Friend
Do not build complex objects directly in your scene. 
* If you are making a desk, locker, or computer, build it once, drag it into the `Assets/Prefabs/Props/` folder to make it a **Prefab** (it will turn blue).
* Use that Prefab in your scene. 
* If you need to change the desk later, double-click the Prefab to open Prefab Mode, make your changes, and save. Git handles Prefab updates much better than Scene updates!

## 🚀 Step 4: Saving Your Work (COMMIT & PUSH)
Done for the day? Time to send your work to GitHub.
1. Save your scene in Unity (`Ctrl+S` or `Cmd+S`).
2. Close Unity (optional, but highly recommended before pushing to avoid locked files).
3. Open your Git client.
4. **Review your changes.** Did you accidentally modify `MainScene`? If yes, discard those changes before committing!
5. **Write a clear Commit Message.** * *Bad:* "did stuff"
   * *Good:* "Added 5 tables and chairs to the CafeteriaScene"
6. **COMMIT** your changes to your branch.
7. **PUSH** your branch to origin.

## 🤝 Step 5: Open a Pull Request (PR)
When your feature or area is totally done and ready to be added to the official game, it's time for a PR.
1. Go to the repository page on GitHub.com.
2. You will usually see a green button that says **"Compare & pull request"** next to your recently pushed branch. Click it.
3. Give your PR a clear title (e.g., "Finished Cafeteria Tables").
4. Add a brief description of what you changed.
5. Click **Create Pull Request**.
6. **Wait for Review.** Do not merge it yourself! The club advisor or lead programmer will review your changes, approve them, and merge them into `main`.

## ⚠️ Uh Oh... I got a Merge Conflict!
**Do not panic, and do not force the push.**
If Git yells at you about a conflict, tell the club advisor immediately. We can easily fix it together, but guessing on a Git conflict can accidentally delete someone else's work.