using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : SettingsUI
{
    [Header("Menu Specific UI")]
    private bool isLoggedIn = true;

    [Header("Flag Selection")]
    [SerializeField] private GameObject flagSelectionCanvas;
    [SerializeField] private Button flagButton;
    [SerializeField] private Button flagCloseButton;

    public static bool IsFlagSelectionOpen { get; private set; } = false;

    protected override void Start()
    {
        base.Start();
        InitializeFlagSelection();
    }

    private void InitializeFlagSelection()
    {
        if (flagSelectionCanvas != null)
        {
            flagSelectionCanvas.SetActive(false);
        }

        if (flagButton != null)
        {
            flagButton.onClick.AddListener(OnFlagButtonClick);
        }

        if (flagCloseButton != null)
        {
            flagCloseButton.onClick.AddListener(OnFlagCloseButtonClick);
        }

        IsFlagSelectionOpen = false;
    }

    public void OnFlagButtonClick()
    {
        OpenFlagSelection();
    }

    public void OnFlagCloseButtonClick()
    {
        CloseFlagSelection();
    }

    public void OpenFlagSelection()
    {
        if (flagSelectionCanvas != null)
        {
            flagSelectionCanvas.SetActive(true);
            IsFlagSelectionOpen = true;
        }
    }

    public void CloseFlagSelection()
    {
        if (flagSelectionCanvas != null)
        {
            flagSelectionCanvas.SetActive(false);
            IsFlagSelectionOpen = false;
        }
    }

    protected override void InitializeActionButton()
    {
        if (actionButton != null)
        {
            // Set up the Log Out/Log In button
            actionButton.onClick.AddListener(OnLogOutLogInButtonClick);
            UpdateActionButtonText();
        }
    }

    private void UpdateActionButtonText()
    {
        if (actionButton != null)
        {
            var buttonText = actionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isLoggedIn ? "Log Out" : "Log In";
            }
        }
    }

    public void OnLogOutLogInButtonClick()
    {
        CloseSettings();

        if (isLoggedIn)
        {
            PerformLogOut();
        }
        else
        {
            PerformLogIn();
        }
    }

    private void PerformLogOut()
    {
        Debug.Log("Logging out...");

        // Add your logout logic here, such as:
        // - Clear user data
        // - Reset player preferences
        // - Clear authentication tokens
        // - etc.

        isLoggedIn = false;
        UpdateActionButtonText();

        // Optionally navigate to a login scene or show login UI
        // SceneManager.LoadScene("LoginScene");

        Debug.Log("User logged out successfully");
    }

    private void PerformLogIn()
    {
        Debug.Log("Attempting to log in...");

        // Add your login logic here, such as:
        // - Show login form
        // - Navigate to login scene
        // - Authenticate with server
        // - etc.

        // For demo purposes, just toggle the state
        // In a real implementation, this would happen after successful authentication
        isLoggedIn = true;
        UpdateActionButtonText();

        // SceneManager.LoadScene("LoginScene");

        Debug.Log("User logged in successfully");
    }

    protected override void CleanupActionButton()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnLogOutLogInButtonClick);
        }
    }

    public override void CloseSettings()
    {
        base.CloseSettings();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (flagButton != null)
        {
            flagButton.onClick.RemoveListener(OnFlagButtonClick);
        }

        if (flagCloseButton != null)
        {
            flagCloseButton.onClick.RemoveListener(OnFlagCloseButtonClick);
        }
    }
}