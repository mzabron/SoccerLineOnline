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

    [Header("Timer Animation Settings")]
    [SerializeField] private float maxTextAlpha = 1f;
    [SerializeField] private float minTextAlpha = 150f / 255f;
    [SerializeField] private float maxPanelAlpha = 50f / 255f;
    [SerializeField] private float minPanelAlpha = 0f;

    [Header("Player 1 Components")]
    [SerializeField] private TMP_Text player1TimerText;
    [SerializeField] private Image player1InnerColorPanel;

    [Header("Player 2 Components")]
    [SerializeField] private TMP_Text player2TimerText;
    [SerializeField] private Image player2InnerColorPanel;

    public static bool IsSettingsOpen { get; private set; } = false;
    public static bool IsAnimationsEnabled { get; private set; } = true;
    public static bool IsBallAnimationEnabled { get; private set; } = true;
    public static bool IsShowBallEnabled { get; private set; } = true;
    private LogicManager logicManager;
    private Color originalPlayer1TextColor;
    private Color originalPlayer1PanelColor;
    private Color originalPlayer2TextColor;
    private Color originalPlayer2PanelColor;

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

        if (player1TimerText != null)
            originalPlayer1TextColor = player1TimerText.color;
        if (player1InnerColorPanel != null)
            originalPlayer1PanelColor = player1InnerColorPanel.color;
        if (player2TimerText != null)
            originalPlayer2TextColor = player2TimerText.color;
        if (player2InnerColorPanel != null)
            originalPlayer2PanelColor = player2InnerColorPanel.color;
    }

    void Update()
    {
        if (logicManager.is1TimerRunning)
        {
            SetPlayer1Active();
            SetPlayer2ToInactive();
        }
        else if (logicManager.is2TimerRunning)
        {
            SetPlayer2Active();
            SetPlayer1ToInactive();
        }
        else
        {
            SetAllToInactiveState();
        }
    }

    private void SetPlayer1Active()
    {
        if (player1TimerText != null)
        {
            Color textColor = originalPlayer1TextColor;
            textColor.a = maxTextAlpha;
            player1TimerText.color = textColor;
        }

        if (player1InnerColorPanel != null)
        {
            Color panelColor;
            if (logicManager.isPlayer1TimeLow)
            {
                panelColor = Color.red;
            }
            else
            {
                panelColor = originalPlayer1PanelColor;
            }
            panelColor.a = maxPanelAlpha;
            player1InnerColorPanel.color = panelColor;
        }
    }

    private void SetPlayer2Active()
    {
        if (player2TimerText != null)
        {
            Color textColor = originalPlayer2TextColor;
            textColor.a = maxTextAlpha;
            player2TimerText.color = textColor;
        }

        if (player2InnerColorPanel != null)
        {
            Color panelColor;
            if (logicManager.isPlayer2TimeLow)
            {
                panelColor = Color.red;
            }
            else
            {
                panelColor = originalPlayer2PanelColor;
            }
            panelColor.a = maxPanelAlpha;
            player2InnerColorPanel.color = panelColor;
        }
    }

    private void SetPlayer1ToInactive()
    {
        if (player1TimerText != null)
        {
            Color textColor = originalPlayer1TextColor;
            textColor.a = minTextAlpha;
            player1TimerText.color = textColor;
        }

        if (player1InnerColorPanel != null)
        {
            Color panelColor;
            if (logicManager.isPlayer1TimeLow)
            {
                panelColor = Color.red;
            }
            else
            {
                panelColor = originalPlayer1PanelColor;
            }
            panelColor.a = minPanelAlpha;
            player1InnerColorPanel.color = panelColor;
        }
    }

    private void SetPlayer2ToInactive()
    {
        if (player2TimerText != null)
        {
            Color textColor = originalPlayer2TextColor;
            textColor.a = minTextAlpha;
            player2TimerText.color = textColor;
        }

        if (player2InnerColorPanel != null)
        {
            Color panelColor;
            if (logicManager.isPlayer2TimeLow)
            {
                panelColor = Color.red;
            }
            else
            {
                panelColor = originalPlayer2PanelColor;
            }
            panelColor.a = minPanelAlpha;
            player2InnerColorPanel.color = panelColor;
        }
    }

    private void SetAllToInactiveState()
    {
        SetPlayer1ToInactive();
        SetPlayer2ToInactive();
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