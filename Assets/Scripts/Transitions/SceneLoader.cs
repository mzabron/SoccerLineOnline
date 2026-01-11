using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    [SerializeField] private SceneFader sceneFader;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
            return;
        }

        if (sceneFader == null)
        {
            sceneFader = transform.root.GetComponentInChildren<SceneFader>();
        }
    }

    public void ChangeSceneTo(string sceneName)
    {
        StartCoroutine(ChangeSceneCoroutine(sceneName));
    }

    private IEnumerator ChangeSceneCoroutine(string sceneName)
    {

        if (sceneFader != null)
        {
            sceneFader.DoFadeOut();
            if (sceneFader.changeAlphaCoroutine != null)
            {
                yield return sceneFader.changeAlphaCoroutine;
            }
        }
        yield return new WaitForEndOfFrame();
        yield return null; 

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (sceneFader != null)
        {
            sceneFader.DoFadeIn();
        }
    }
}
