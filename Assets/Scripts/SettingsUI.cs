using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] protected GameObject settingsCanvas;
    [SerializeField] protected Button settingsButton;
    [SerializeField] protected Button cancelButton;
    [SerializeField] protected Toggle animationsToggle;
    [SerializeField] protected Toggle ballAnimationToggle;
    [SerializeField] protected Toggle showBallToggle;
    [SerializeField] protected Toggle allowSwipeMoveToggle;

    [Header("Action Button (Scene Specific)")]
    [SerializeField] protected Button actionButton; //Restart in Game, Log Out/In in Menu

    public static bool IsSettingsOpen { get; private set; } = false;
    public static bool IsAnimationsEnabled { get; private set; } = true;
    public static bool IsBallAnimationEnabled { get; private set; } = true;
    public static bool IsShowBallEnabled { get; private set; } = true;
    public static bool IsSwipeMoveEnabled { get; private set; } = true;

    protected virtual void Start()
    {
        InitializeSettingsUI();
        InitializeToggles();
        InitializeActionButton();
    }

    protected virtual void InitializeSettingsUI()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CloseSettings);
        }

        IsSettingsOpen = false;
    }

    protected virtual void InitializeActionButton()
    {
        // To override
    }

    protected virtual void InitializeToggles()
    {
        InitializeAnimationsToggle();
        InitializeBallAnimationToggle();
        InitializeShowBallToggle();
        InitializeAllowSwipeMoveToggle();
    }

    public virtual void OnSettingsButtonClick()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(true);
        }
        IsSettingsOpen = true;
    }

    public virtual void CloseSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
        }

        if (settingsButton != null)
        {
            settingsButton.gameObject.SetActive(true);
        }

        IsSettingsOpen = false;
    }

    protected void InitializeAnimationsToggle()
    {
        if (animationsToggle == null) return;

        animationsToggle.isOn = IsAnimationsEnabled;
        animationsToggle.onValueChanged.AddListener(OnAnimationsToggleChanged);
    }

    protected void InitializeBallAnimationToggle()
    {
        if (ballAnimationToggle == null) return;

        ballAnimationToggle.isOn = IsBallAnimationEnabled;
        ballAnimationToggle.onValueChanged.AddListener(OnBallAnimationToggleChanged);
    }

    protected void InitializeShowBallToggle()
    {
        if (showBallToggle == null) return;

        showBallToggle.isOn = IsShowBallEnabled;
        showBallToggle.onValueChanged.AddListener(OnShowBallToggleChanged);
    }

    protected void InitializeAllowSwipeMoveToggle()
    {
        if (allowSwipeMoveToggle == null) return;

        allowSwipeMoveToggle.isOn = IsSwipeMoveEnabled;
        allowSwipeMoveToggle.onValueChanged.AddListener(OnAllowSwipeMoveToggleChanged);
    }

    protected virtual void OnAnimationsToggleChanged(bool isEnabled)
    {
        IsAnimationsEnabled = isEnabled;
        Debug.Log($"Line animations {(isEnabled ? "enabled" : "disabled")}");
    }

    protected virtual void OnBallAnimationToggleChanged(bool isEnabled)
    {
        IsBallAnimationEnabled = isEnabled;
        Debug.Log($"Ball animations {(isEnabled ? "enabled" : "disabled")}");
    }

    protected virtual void OnShowBallToggleChanged(bool isEnabled)
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

    protected virtual void OnAllowSwipeMoveToggleChanged(bool isEnabled)
    {
        IsSwipeMoveEnabled = isEnabled;
    }

    protected virtual void OnDestroy()
    {
        if (animationsToggle != null)
            animationsToggle.onValueChanged.RemoveListener(OnAnimationsToggleChanged);
        if (ballAnimationToggle != null)
            ballAnimationToggle.onValueChanged.RemoveListener(OnBallAnimationToggleChanged);
        if (showBallToggle != null)
            showBallToggle.onValueChanged.RemoveListener(OnShowBallToggleChanged);
        if (allowSwipeMoveToggle != null)
            allowSwipeMoveToggle.onValueChanged.RemoveListener(OnAllowSwipeMoveToggleChanged);
        CleanupActionButton();
    }

    protected virtual void CleanupActionButton()
    {
    }
}