using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _canvas;
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
        _canvas.SetActive(false);

        bool finished = false;

        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = Application.streamingAssetsPath + "/video/automatisationcutscene_vp8.webm";
        //_videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
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
