using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SettingsUI
{
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
    protected override void Start()
    {
        base.Start();
        logicManager = FindFirstObjectByType<LogicManager>();
        InitializeGameUI();

        if (player1TimerText != null)
            originalPlayer1TextColor = player1TimerText.color;
        if (player1InnerColorPanel != null)
            originalPlayer1PanelColor = player1InnerColorPanel.color;
        if (player2TimerText != null)
            originalPlayer2TextColor = player2TimerText.color;
        if (player2InnerColorPanel != null)
            originalPlayer2PanelColor = player2InnerColorPanel.color;
    }

    protected override void InitializeActionButton()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnRestartGameButtonClick);
            var buttonText = actionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Restart";
            }
        }
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

    private void InitializeGameUI()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
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
                winnerMessageText.text = $"{winnerNickname} won!";
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
    protected override void CleanupActionButton()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnRestartGameButtonClick);
        }
    }
}