using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager instance;

    [Header("Quit UI")]
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeQuitUI();
    }

    private void InitializeQuitUI()
    {
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);
        
        if (quitYesButton != null)
            quitYesButton.onClick.AddListener(OnQuitYesButtonClick);
        
        if (quitNoButton != null)
            quitNoButton.onClick.AddListener(OnQuitNoButtonClick);

        if (quitPanel != null)
            quitPanel.SetActive(false);
    }

    public void OnGetStartedButtonClicked()
    {
        NavigateToMainMenu();
    }

    public void OnExitButtonClick()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(true);
        }
    }

    public void OnQuitYesButtonClick()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }

        NavigateToMainMenu();
    }

    public void OnQuitNoButtonClick()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
    }

    private void NavigateToMainMenu()
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

    private void OnDestroy()
    {
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitButtonClick);
        
        if (quitYesButton != null)
            quitYesButton.onClick.RemoveListener(OnQuitYesButtonClick);
        
        if (quitNoButton != null)
            quitNoButton.onClick.RemoveListener(OnQuitNoButtonClick);
    }
}

