using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }
    public IEnumerator LoadGameCoroutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
        asyncLoad.allowSceneActivation = false;

        Resources.LoadAll("MapAssets");
        yield return null;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return null;

        // Get the NavMeshSurface from the loaded scene
        Scene mainScene = SceneManager.GetSceneByName("Game");
        foreach (GameObject go in mainScene.GetRootGameObjects())
        {
            NavMeshSurface surface = go.GetComponentInChildren<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                break;
            }
        }

        yield return null;

        Destroy(gameObject);
    }
}
