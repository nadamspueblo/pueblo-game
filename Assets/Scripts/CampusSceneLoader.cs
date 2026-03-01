using UnityEngine;
using UnityEngine.SceneManagement;

public class CampusSceneLoader : MonoBehaviour
{
    [Header("Campus Zones")]
    [Tooltip("Type the exact names of the scenes to load additively (e.g., CafeteriaScene)")]
    public string[] scenesToLoad;

    void Start()
    {
        LoadAllCampusZones();
    }

    private void LoadAllCampusZones()
    {
        foreach (string sceneName in scenesToLoad)
        {
            // First, check if the scene is already open in the Editor
            // This prevents duplicate loading if you are actively working on it
            if (!IsSceneAlreadyLoaded(sceneName))
            {
                // Load the scene additively in the background
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }
    }

    private bool IsSceneAlreadyLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}