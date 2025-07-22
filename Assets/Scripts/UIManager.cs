using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsCanva;
    [SerializeField] private Button settingsButton;

    public static bool IsSettingsOpen { get; private set; } = false;
    private LogicManager logicManager;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Debug.Log("Default frame rate set to 60 FPS on Awake");
    }
    void Start()
    {
        InitializeUI();
        logicManager = FindFirstObjectByType<LogicManager>();
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
}