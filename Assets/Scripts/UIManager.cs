using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsCanva;
    [SerializeField] private Button settingsButton;
    [SerializeField] private TMP_Dropdown refreshRateDropdown;

    public static bool IsSettingsOpen { get; private set; } = false;
    private LogicManager logicManager;

    private readonly int[] availableRefreshRates = { 30, 60, 90, 120, 144 };
    private readonly string[] refreshRateOptions = { "30 FPS", "60 FPS", "90 FPS", "120 FPS", "144 FPS", "Unlimited" };

    void Start()
    {
        InitializeUI();
        logicManager = FindFirstObjectByType<LogicManager>();
        InitializeRefreshRateDropdown();
    }

    private void InitializeUI()
    {
        if (settingsCanva != null)
        {
            settingsCanva.SetActive(false);
        }
        if (settingsButton != null)
        {
            settingsButton.gameObject.SetActive(true);
        }

        IsSettingsOpen = false;
    }

    private void InitializeRefreshRateDropdown()
    {
        if (refreshRateDropdown == null)
        {
            Debug.LogWarning("Refresh Rate Dropdown is not assigned in UIManager!");
            return;
        }

        // Clear existing options
        refreshRateDropdown.ClearOptions();

        // Add refresh rate options to dropdown
        List<string> options = new List<string>(refreshRateOptions);
        refreshRateDropdown.AddOptions(options);

        // Set default value (60 FPS is most common)
        int defaultIndex = GetRefreshRateIndex(60);
        refreshRateDropdown.value = defaultIndex;
        refreshRateDropdown.RefreshShownValue();

        // Apply the default refresh rate
        ApplyRefreshRate(defaultIndex);

        // Subscribe to dropdown value change event
        refreshRateDropdown.onValueChanged.AddListener(OnRefreshRateChanged);

        Debug.Log($"Refresh rate dropdown initialized with {options.Count} options");
    }

    private int GetRefreshRateIndex(int targetFPS)
    {
        for (int i = 0; i < availableRefreshRates.Length; i++)
        {
            if (availableRefreshRates[i] == targetFPS)
                return i;
        }
        return 1; // Default to 60 FPS if not found
    }

    private void OnRefreshRateChanged(int selectedIndex)
    {
        ApplyRefreshRate(selectedIndex);
        Debug.Log($"Refresh rate changed to: {refreshRateOptions[selectedIndex]}");
    }

    private void ApplyRefreshRate(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < refreshRateOptions.Length)
        {
            if (selectedIndex == refreshRateOptions.Length - 1)
            {
                // "Unlimited" option - let the device run at maximum refresh rate
                Application.targetFrameRate = 0;
                Debug.Log("Target frame rate set to unlimited (0)");
            }
            else if (selectedIndex < availableRefreshRates.Length)
            {
                // Set specific frame rate
                int targetFPS = availableRefreshRates[selectedIndex];
                Application.targetFrameRate = targetFPS;
                Debug.Log($"Target frame rate set to: {targetFPS} FPS");
            }
        }
    }

    public void OnSettingsButtonClick()
    {
        if (settingsCanva != null)
        {
            settingsCanva.SetActive(true);
        }
        if (settingsButton != null)
        {
            settingsButton.gameObject.SetActive(false);
        }

        IsSettingsOpen = true;
    }

    public void CloseSettings()
    {
        if (settingsCanva != null)
        {
            settingsCanva.SetActive(false);
        }
        if (settingsButton != null)
        {
            settingsButton.gameObject.SetActive(true);
        }

        IsSettingsOpen = false;
    }

    public void OnRestartGameButtonClick()
    {
        CloseSettings();

        if (logicManager != null)
        {
            logicManager.RestartGame();
        }
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (refreshRateDropdown != null)
        {
            refreshRateDropdown.onValueChanged.RemoveListener(OnRefreshRateChanged);
        }
    }

    public int GetCurrentRefreshRateSetting()
    {
        if (refreshRateDropdown != null)
        {
            return refreshRateDropdown.value;
        }
        return 1; // Default to 60 FPS
    }
    public void SetRefreshRate(int refreshRateIndex)
    {
        if (refreshRateDropdown != null && refreshRateIndex >= 0 && refreshRateIndex < refreshRateOptions.Length)
        {
            refreshRateDropdown.value = refreshRateIndex;
            refreshRateDropdown.RefreshShownValue();
            ApplyRefreshRate(refreshRateIndex);
        }
    }
}