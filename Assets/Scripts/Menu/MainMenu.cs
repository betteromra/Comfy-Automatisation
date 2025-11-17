using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] RawImage _rawImage;
    [SerializeField] VideoPlayer _videoPlayer;
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
        _rawImage.gameObject.SetActive(true);

        bool finished = false;

        _videoPlayer.loopPointReached += (vp) => finished = true;
        _videoPlayer.Play();

        yield return new WaitUntil(() => finished);
        
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
