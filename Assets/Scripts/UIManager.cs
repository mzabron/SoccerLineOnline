using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] private GameObject settingsCanva;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Toggle animationsToggle; //line drawing animation
    [SerializeField] private Toggle ballAnimationToggle;
    [SerializeField] private Toggle showBallToggle;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TMP_Text winnerMessageText;
    [SerializeField] private TMP_Text winnerRatingText;

    public static bool IsSettingsOpen { get; private set; } = false;
    public static bool IsAnimationsEnabled { get; private set; } = true;
    public static bool IsBallAnimationEnabled { get; private set; } = true;
    public static bool IsShowBallEnabled { get; private set; } = true;
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
        InitializeAnimationsToggle();
        InitializeBallAnimationToggle();
        InitializeShowBallToggle();
    }

    private void InitializeShowBallToggle()
    {
        if (showBallToggle == null)
        {
            Debug.LogWarning("Show Ball Toggle is not assigned in UIManager!");
            return;
        }

        showBallToggle.isOn = IsShowBallEnabled;
        showBallToggle.onValueChanged.AddListener(OnShowBallToggleChanged);

        Debug.Log($"Show Ball toggle initialized. Show Ball enabled: {IsShowBallEnabled}");
    }

    private void OnShowBallToggleChanged(bool isEnabled)
    {
        IsShowBallEnabled = isEnabled;

        if (!isEnabled)
        {
            IsBallAnimationEnabled = false;
            if (ballAnimationToggle != null)
            {
                ballAnimationToggle.isOn = false;
                ballAnimationToggle.interactable = false;
            }
        }
        else
        {
            if (ballAnimationToggle != null)
            {
                ballAnimationToggle.interactable = true;
                ballAnimationToggle.isOn = true;
                IsBallAnimationEnabled = true;
            }
        }

        Debug.Log($"Show Ball {(isEnabled ? "enabled" : "disabled")}. Ball animations also {(IsBallAnimationEnabled ? "enabled" : "disabled")}");
    }

    private void InitializeBallAnimationToggle()
    {
        if (ballAnimationToggle == null)
        {
            return;
        }

        ballAnimationToggle.isOn = IsBallAnimationEnabled;
        ballAnimationToggle.onValueChanged.AddListener(OnBallAnimationToggleChanged);

        Debug.Log($"Ball animations toggle initialized. Ball animations enabled: {IsBallAnimationEnabled}");
    }

    private void OnBallAnimationToggleChanged(bool isEnabled)
    {
        IsBallAnimationEnabled = isEnabled;
        Debug.Log($"Ball animations {(isEnabled ? "enabled" : "disabled")}");
    }

    private void InitializeAnimationsToggle()
    {
        animationsToggle.isOn = IsAnimationsEnabled;
        animationsToggle.onValueChanged.AddListener(OnAnimationsToggleChanged);

        Debug.Log($"Animations toggle initialized. Animations enabled: {IsAnimationsEnabled}");
    }

    private void OnAnimationsToggleChanged(bool isEnabled)
    {
        IsAnimationsEnabled = isEnabled;
        Debug.Log($"Line animations {(isEnabled ? "enabled" : "disabled")}");
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
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
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

    public void ShowGameOverScreen()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            Debug.Log("Game Over screen displayed");
        }
    }
    public void ShowGameOverScreen(string winnerNickname, int winnerRating)
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);

            if (winnerMessageText != null)
            {
                winnerMessageText.text = $"{winnerNickname} has won!";
            }

            if (winnerRatingText != null)
            {
                winnerRatingText.text = $"{winnerRating}";
            }

            Debug.Log($"Game Over screen displayed - Winner: {winnerNickname} (Rating: {winnerRating})");
        }
        else
        {
            Debug.LogWarning("GameOverCanvas is not assigned in UIManager!");
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
            Debug.Log("Game Over screen hidden");
        }
    }

    void OnDestroy()
    {
        if (animationsToggle != null)
        {
            animationsToggle.onValueChanged.RemoveListener(OnAnimationsToggleChanged);
        }
        if (ballAnimationToggle != null)
        {
            ballAnimationToggle.onValueChanged.RemoveListener(OnBallAnimationToggleChanged);
        }
        if (showBallToggle != null)
        {
            showBallToggle.onValueChanged.RemoveListener(OnShowBallToggleChanged);
        }
    }
}