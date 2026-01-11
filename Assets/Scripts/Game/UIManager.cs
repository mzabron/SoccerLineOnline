using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : SettingsUI
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text winnerMessageText;
    [SerializeField] private TMP_Text winnerRatingText;
    [SerializeField] private Button newGameButton;

    [Header("Quit UI")]
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;

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


    public static bool IsQuitPanelOpen { get; private set; } = false;

    protected override void Start()
    {
        base.Start();
        // Reset flags on start to be safe
        IsQuitPanelOpen = false;
        
        logicManager = FindFirstObjectByType<LogicManager>();
        InitializeGameUI();
        InitializeQuitUI();

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

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnRestartGameButtonClick);
        }
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

    public void OnRestartGameButtonClick()
    {
        CloseSettings();

        if (logicManager != null)
        {
            logicManager.RestartGame();
        }
    }

    public void OnExitButtonClick()
    {
        IsQuitPanelOpen = true;

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        if (quitPanel != null) quitPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void OnQuitYesButtonClick()
    {
        IsQuitPanelOpen = false;

        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }

        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.ChangeSceneTo("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnQuitNoButtonClick()
    {
        IsQuitPanelOpen = false;

        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (quitPanel != null) quitPanel.SetActive(false);
            Debug.Log("Game Over screen displayed");
        }
    }

    public void ShowGameOverScreen(string winnerNickname, int winnerRating)
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (quitPanel != null) quitPanel.SetActive(false);


            IsQuitPanelOpen = false; 

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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        

        IsQuitPanelOpen = false;

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitButtonClick);
        
        if (quitYesButton != null)
            quitYesButton.onClick.RemoveListener(OnQuitYesButtonClick);
        
        if (quitNoButton != null)
            quitNoButton.onClick.RemoveListener(OnQuitNoButtonClick);

        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnRestartGameButtonClick);
    }
}