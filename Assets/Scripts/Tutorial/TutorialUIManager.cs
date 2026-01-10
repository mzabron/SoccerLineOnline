using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager instance;
    private SceneFader sceneFader;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeSceneTo(string sceneName)
    {
        StartCoroutine(ChangeSceneCoroutine(sceneName));
    }

    public void OnGetStartedButtonClicked()
    {
        ChangeSceneTo("MainMenu");
    }

    private IEnumerator ChangeSceneCoroutine(string sceneName)
    {
        GetSceneFader().DoFadeOut();
        yield return GetSceneFader().changeAlphaCoroutine;
        SceneManager.LoadScene(sceneName);
    }

    private SceneFader GetSceneFader()
    {
        if(sceneFader == null)
        {
            sceneFader = FindFirstObjectByType<SceneFader>();
        }

        return sceneFader;
    }


}

