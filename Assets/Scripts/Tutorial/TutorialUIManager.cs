using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnGetStartedButtonClicked()
    {
        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.ChangeSceneTo("MainMenu");
        }
        else
        {
            Debug.LogWarning("SceneLoader not found. Loading scene directly.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}

