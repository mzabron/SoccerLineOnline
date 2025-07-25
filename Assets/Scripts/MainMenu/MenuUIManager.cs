using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : SettingsUI
{
    [Header("Menu Specific UI")]

    private bool isLoggedIn = true;

    protected override void Start()
    {
        base.Start();
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
}