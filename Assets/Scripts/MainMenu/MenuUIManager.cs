using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MenuUIManager : SettingsUI
{
    [Header("Menu Specific UI")]
    private bool isLoggedIn = true;

    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private string defaultPlayerName = "Player";

    [Header("Flag Selection")]
    [SerializeField] private GameObject flagSelectionCanvas;
    [SerializeField] private Button flagButton;
    [SerializeField] private Button flagCloseButton;
    [SerializeField] private Transform flagGridParent; // Content
    [SerializeField] private GameObject flagButtonPrefab;

    [Header("Flag Grid Settings")]
    [SerializeField] private Vector2 flagButtonSize = new Vector2(80f, 60f);
    [SerializeField] private Vector2 flagSpacing = new Vector2(10f, 10f);
    [SerializeField] private RectOffset gridPadding;
    
    [Header("Flag Scaling Settings")]
    [SerializeField] private Vector2 flagScaleFactors = new Vector2(0.9187472f, 0.428239f); //parent panel Scale

    [Header("Localhost Panel")]
    [SerializeField] private Button local2PlayersButton;
    [SerializeField] private GameObject localhostPanel;
    [SerializeField] private Button localhostCancelButton;
    [SerializeField] private Button localhostPlayButton;
    [SerializeField] private Toggle noTimeToggle;
    [SerializeField] private TMP_Text timeSelectionText;
    [SerializeField] private Button timeLeftArrowButton;
    [SerializeField] private Button timeRightArrowButton;

    [Header("Localhost Animation")]
    [SerializeField] private CarouselTextAnimation localhostTimeAnimator;

    [Header("Hint Panel")]
    [SerializeField] private Button hintButton;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private Button hintCancelButton;
    [SerializeField] private Button startTutorialButton;

    [Header("Game Mode Buttons")]
    [SerializeField] private Button playOnlineButton;
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button playWithFriendButton;

    [Header("Coming Soon Settings")]
    [SerializeField] private GameObject comingSoonPanel;
    [SerializeField] private float comingSoonDisplayDuration = 1.0f;
    [SerializeField] private float comingSoonFadeDuration = 0.5f;

    private List<GameObject> instantiatedFlagButtons = new List<GameObject>();
    private GridLayoutGroup gridLayout;
    private RectTransform gridRectTransform;
    private ScrollRect flagScrollRect;
    private Vector2 lastFlagButtonSize;
    private Vector2 lastFlagSpacing;
    private RectOffset lastGridPadding;
    private float lastPanelWidth;

    // Localhost Panel Variables
    private readonly int[] timeOptions = { 1, 3, 5, 10 };
    private int currentTimeIndex = 0;

    // Coming Soon Variables
    private Coroutine comingSoonCoroutine;
    private CanvasGroup comingSoonCanvasGroup;
    private RectTransform comingSoonRectTransform;
    private MonoBehaviour conflictingAnimator;

    public static bool IsFlagSelectionOpen { get; private set; } = false;

    void Awake()
    {
        // TEMPORARY: Clear PlayerPrefs in Awake to ensure it happens before other scripts' Start()
        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.Save();
    }

    protected override void Start()
    {
        base.Start();
        InitializeFlagSelection();
        LoadSelectedFlag();
        LoadPlayerName();
        InitializeLocalhostPanel();
        InitializeHintPanel();
        InitializeGameModeButtons();
        InitializeComingSoonPanel();

        // Auto-detect animator if not assigned but text exists
        if (localhostTimeAnimator == null && timeSelectionText != null)
        {
            localhostTimeAnimator = timeSelectionText.GetComponent<CarouselTextAnimation>();
        }
    }

    public void LoadPlayerName()
    {
        if (playerNameText != null)
        {
            string savedPlayerName = PlayerPrefs.GetString("PlayerName", defaultPlayerName);
            playerNameText.text = savedPlayerName;
        }
        else
        {
            Debug.LogWarning("PlayerNameText is not assigned in MenuUIManager!");
        }
    }

    private void LoadSelectedFlag()
    {
        string savedFlagName = PlayerPrefs.GetString("SelectedFlag", "");
        
        if (string.IsNullOrEmpty(savedFlagName))
        {
            Debug.Log("No saved flag found in PlayerPrefs");
            return;
        }

        Sprite savedFlagSprite = null;
        Sprite[] allFlagSprites = Resources.LoadAll<Sprite>("Flags");
        
        foreach (Sprite sprite in allFlagSprites)
        {
            if (sprite.name == savedFlagName)
            {
                savedFlagSprite = sprite;
                break;
            }
        }
        
        if (savedFlagSprite == null)
        {
            return;
        }
        
        if (flagButton == null)
        {
            Debug.LogError("flagButton is null - cannot load saved flag!");
            return;
        }

        Image mainFlagImage = flagButton.GetComponent<Image>();
        if (mainFlagImage != null)
        {
            mainFlagImage.sprite = savedFlagSprite;
            mainFlagImage.preserveAspect = true;
            Debug.Log($"Successfully loaded saved flag: {savedFlagName}");
        }
        else
        {
            Debug.LogError("Flag button does not have an Image component!");
        }
    }

    private void Update()
    {
        if (gridLayout != null && gridRectTransform != null)
        {
            bool needsUpdate = false;
            if (lastFlagButtonSize != flagButtonSize)
            {
                lastFlagButtonSize = flagButtonSize;
                needsUpdate = true;
            }

            if (lastFlagSpacing != flagSpacing)
            {
                lastFlagSpacing = flagSpacing;
                needsUpdate = true;
            }

            if (!RectOffsetEquals(lastGridPadding, gridPadding))
            {
                lastGridPadding = new RectOffset(gridPadding.left, gridPadding.right, gridPadding.top, gridPadding.bottom);
                needsUpdate = true;
            }

            float currentPanelWidth = gridRectTransform.rect.width;
            if (Mathf.Abs(lastPanelWidth - currentPanelWidth) > 0.1f)
            {
                lastPanelWidth = currentPanelWidth;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                ConfigureGridLayout();
                UpdateFlagScaling();
            }
        }
    }

    private bool RectOffsetEquals(RectOffset a, RectOffset b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.left == b.left && a.right == b.right && a.top == b.top && a.bottom == b.bottom;
    }

    private void UpdateFlagScaling()
    {
        foreach (GameObject flagButtonObj in instantiatedFlagButtons)
        {
            if (flagButtonObj != null)
            {
                ApplyFlagScaling(flagButtonObj);
            }
        }
    }

    private void ApplyFlagScaling(GameObject flagButtonObj)
    {
        Image flagImage = null;
        Image[] childImages = flagButtonObj.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.gameObject != flagButtonObj)
            {
                flagImage = img;
                break;
            }
        }

        if (flagImage == null)
        {
            flagImage = flagButtonObj.GetComponent<Image>();
        }

        if (flagImage != null)
        {
            RectTransform imageRect = flagImage.GetComponent<RectTransform>();
            if (imageRect != null)
            {
                imageRect.localScale = new Vector3(
                    flagScaleFactors.x,
                    flagScaleFactors.y,
                    1.0f
                );
            }
        }
    }

    private void ResetScrollToTop()
    {
        if (flagScrollRect != null)
        {
            flagScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void InitializeFlagSelection()
    {
        if (flagSelectionCanvas != null)
        {
            flagSelectionCanvas.SetActive(false);
            flagScrollRect = flagSelectionCanvas.GetComponentInChildren<ScrollRect>();
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
        
        if (flagGridParent != null)
        {
            gridLayout = flagGridParent.GetComponent<GridLayoutGroup>();
            gridRectTransform = flagGridParent.GetComponent<RectTransform>();
        }

        ConfigureGridLayout();
        LoadFlagIcons();

        lastFlagButtonSize = flagButtonSize;
        lastFlagSpacing = flagSpacing;
        if (gridPadding != null)
        {
            lastGridPadding = new RectOffset(gridPadding.left, gridPadding.right, gridPadding.top, gridPadding.bottom);
        }
        if (gridRectTransform != null)
        {
            lastPanelWidth = gridRectTransform.rect.width;
        }
    }

    private void ConfigureGridLayout()
    {
        if (gridLayout == null || gridRectTransform == null || gridPadding == null) return;

        gridLayout.cellSize = flagButtonSize;
        gridLayout.spacing = flagSpacing;
        gridLayout.padding = gridPadding;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;

        float availableWidth = gridRectTransform.rect.width;
        float cellWidth = flagButtonSize.x;
        float spacingWidth = flagSpacing.x;

        availableWidth -= (gridPadding.left + gridPadding.right);

        int maxColumns = Mathf.FloorToInt((availableWidth + spacingWidth) / (cellWidth + spacingWidth));
        maxColumns = Mathf.Max(1, maxColumns);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxColumns;
    }

    private void LoadFlagIcons()
    {
        if (flagGridParent == null || flagButtonPrefab == null) return;

        ClearFlagButtons();
        Sprite[] flagSprites = Resources.LoadAll<Sprite>("Flags");

        foreach (Sprite flagSprite in flagSprites)
        {
            GameObject flagButtonObj = Instantiate(flagButtonPrefab, flagGridParent);
            
            RectTransform buttonRect = flagButtonObj.GetComponent<RectTransform>();
            Vector2 originalButtonSize = buttonRect.sizeDelta;

            Image flagImage = flagButtonObj.GetComponent<Image>();
            if (flagImage != null)
            {
                flagImage.sprite = flagSprite;
                flagImage.preserveAspect = true;
            }

            TMPro.TextMeshProUGUI flagNameText = flagButtonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (flagNameText != null)
            {
                RectTransform textRect = flagNameText.GetComponent<RectTransform>();
                
                Vector2 originalTextSize = textRect.sizeDelta;
                Vector3 originalTextPosition = textRect.localPosition;
                float originalFontSize = flagNameText.fontSize;
                Vector2 scaleFactors = new Vector2(
                    flagButtonSize.x / originalButtonSize.x,
                    flagButtonSize.y / originalButtonSize.y
                );
                
                buttonRect.sizeDelta = flagButtonSize;
                
                textRect.sizeDelta = new Vector2(
                    originalTextSize.x * scaleFactors.x,
                    originalTextSize.y * scaleFactors.y
                );
                
                textRect.localPosition = new Vector3(
                    originalTextPosition.x * scaleFactors.x,
                    originalTextPosition.y * scaleFactors.y,
                    originalTextPosition.z
                );
                
                flagNameText.fontSize = originalFontSize * Mathf.Min(scaleFactors.x, scaleFactors.y);
                
                flagNameText.text = FormatFlagName(flagSprite.name);
            }
            else
            {
                buttonRect.sizeDelta = flagButtonSize;
                Debug.LogWarning($"TextMeshProUGUI component not found in flag button prefab for flag: {flagSprite.name}");
            }

            ApplyFlagScaling(flagButtonObj);
            
            Button button = flagButtonObj.GetComponent<Button>();
            if (button != null)
            {
                string flagName = flagSprite.name;
                button.onClick.AddListener(() => OnFlagSelected(flagName));
            }

            instantiatedFlagButtons.Add(flagButtonObj);
        }

        if (gridLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridRectTransform);
        }
    }

    private string FormatFlagName(string rawFlagName)
    {      
        string formatted = rawFlagName.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
        
        if (formatted.Length > 2)
        {
            formatted = formatted.Substring(0, formatted.Length - 2);
        }
        
        formatted = formatted.Replace("_", " ");
        string[] words = formatted.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        
        return string.Join(" ", words);
    }

    private void ClearFlagButtons()
    {
        foreach (GameObject flagButton in instantiatedFlagButtons)
        {
            if (flagButton != null)
            {
                DestroyImmediate(flagButton);
            }
        }
        instantiatedFlagButtons.Clear();
    }

    private void OnFlagSelected(string flagName)
    {
        Debug.Log($"Flag selected: {flagName}");
        
        Sprite selectedFlagSprite = null;
        Sprite[] allFlagSprites = Resources.LoadAll<Sprite>("Flags");
        
        foreach (Sprite sprite in allFlagSprites)
        {
            if (sprite.name == flagName)
            {
                selectedFlagSprite = sprite;
                break;
            }
        }

        if (selectedFlagSprite == null)
        {
            Debug.LogError($"Could not find flag sprite with name: {flagName}");
            return;
        }

        if (flagButton == null)
        {
            Debug.LogError("flagButton is null!");
            return;
        }

        Image mainFlagImage = null;
        mainFlagImage = flagButton.GetComponent<Image>();

        if (mainFlagImage != null)
        {
            mainFlagImage.sprite = selectedFlagSprite;
            mainFlagImage.preserveAspect = true;

        }

        PlayerPrefs.SetString("SelectedFlag", flagName);
        PlayerPrefs.Save();

        CloseFlagSelection();
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
            
            ConfigureGridLayout();
            StartCoroutine(ResetScrollToTopDelayed());
        }
    }

    private System.Collections.IEnumerator ResetScrollToTopDelayed()
    {
        yield return null;
        ResetScrollToTop();
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
        isLoggedIn = false;
        UpdateActionButtonText();
        Debug.Log("User logged out successfully");
    }

    private void PerformLogIn()
    {
        Debug.Log("Attempting to log in...");
        isLoggedIn = true;
        UpdateActionButtonText();
        Debug.Log("User logged in successfully");
    }

    // ============================================
    // Localhost Panel Logic
    // ============================================

    private void InitializeLocalhostPanel()
    {
        if (local2PlayersButton != null)
            local2PlayersButton.onClick.AddListener(OpenLocalhostPanel);

        if (localhostCancelButton != null)
            localhostCancelButton.onClick.AddListener(CloseLocalhostPanel);

        if (localhostPlayButton != null)
            localhostPlayButton.onClick.AddListener(OnLocalhostPlayButtonClick);

        if (noTimeToggle != null)
        {
            noTimeToggle.isOn = false; // Unchecked by default
            noTimeToggle.onValueChanged.AddListener(OnNoTimeToggleChanged);
        }

        if (timeLeftArrowButton != null)
            timeLeftArrowButton.onClick.AddListener(() => OnTimeArrowClicked(-1));

        if (timeRightArrowButton != null)
            timeRightArrowButton.onClick.AddListener(() => OnTimeArrowClicked(1));

        if (localhostPanel != null)
            localhostPanel.SetActive(false);

        // Initialize state
        currentTimeIndex = 0; // Default to 1 min
        UpdateLocalhostTimeDisplay();
        
        // Ensure initial visuals are correct
        if (timeSelectionText != null)
        {
            timeSelectionText.alpha = 1f;
        }

        OnNoTimeToggleChanged(false);
    }

    private void OpenLocalhostPanel()
    {
        if (localhostPanel != null) localhostPanel.SetActive(true);
    }

    private void CloseLocalhostPanel()
    {
        if (localhostPanel != null) localhostPanel.SetActive(false);
    }

    private void OnLocalhostPlayButtonClick()
    {
        int selectedTime = timeOptions[currentTimeIndex];
        bool isUnlimited = noTimeToggle.isOn;

        PlayerPrefs.SetInt("Game_TimeMinutes", selectedTime);
        PlayerPrefs.SetInt("Game_TimeUnlimited", isUnlimited ? 1 : 0);
        PlayerPrefs.SetString("Game_Player1Name", "Blue");
        PlayerPrefs.SetString("Game_Player2Name", "Red");
        PlayerPrefs.Save();

        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.ChangeSceneTo("Game");
        }
        else
        {
            SceneManager.LoadScene("Game"); // Fallback
        }
    }

    private void OnNoTimeToggleChanged(bool isNoTimeLimit)
    {
        // If "No Time" is checked, we disable the specific time controls
        // The user can interact if the toggle is UNCHECKED (Time limit exists)
        bool isTimeSelectionEnabled = !isNoTimeLimit;

        if (timeSelectionText != null)
        {
            // Make text gray if disabled, white/normal if enabled
            timeSelectionText.alpha = isTimeSelectionEnabled ? 1f : 0.4f;
        }

        if (timeLeftArrowButton != null) timeLeftArrowButton.interactable = isTimeSelectionEnabled;
        if (timeRightArrowButton != null) timeRightArrowButton.interactable = isTimeSelectionEnabled;
    }

    private void OnTimeArrowClicked(int direction)
    {
        // Define the logic to update content (will serve as callback)
        System.Action updateContent = () => 
        {
            currentTimeIndex += direction;
            
            // Wrap around logic
            if (currentTimeIndex < 0) 
                currentTimeIndex = timeOptions.Length - 1;
            else if (currentTimeIndex >= timeOptions.Length)
                currentTimeIndex = 0;

            UpdateLocalhostTimeDisplay();
        };

        if (localhostTimeAnimator != null)
        {
            // Use the generic animator
            localhostTimeAnimator.AnimateChange(direction, updateContent);
        }
        else
        {
            // Fallback to immediate update if animator is missing
            updateContent.Invoke();
        }
    }

    private void UpdateLocalhostTimeDisplay()
    {
        if (timeSelectionText != null)
        {
            timeSelectionText.text = $"{timeOptions[currentTimeIndex]} min";
        }
    }

    // ============================================
    // Hint Panel Logic
    // ============================================

    private void InitializeHintPanel()
    {
        if (hintButton != null)
            hintButton.onClick.AddListener(OpenHintPanel);

        if (hintCancelButton != null)
            hintCancelButton.onClick.AddListener(CloseHintPanel);

        if (startTutorialButton != null)
            startTutorialButton.onClick.AddListener(OnStartTutorialButtonClick);

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void OpenHintPanel()
    {
        if (hintPanel != null)
            hintPanel.SetActive(true);
    }

    private void CloseHintPanel()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void OnStartTutorialButtonClick()
    {
        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.ChangeSceneTo("Tutorial");
        }
        else
        {
            Debug.LogWarning("SceneLoader not found. Loading Tutorial scene directly.");
            SceneManager.LoadScene("Tutorial");
        }
    }

    // ============================================
    // Game Mode Buttons & Coming Soon Logic
    // ============================================

    private void InitializeGameModeButtons()
    {
        if (playOnlineButton != null)
            playOnlineButton.onClick.AddListener(ShowComingSoon);

        if (singlePlayerButton != null)
            singlePlayerButton.onClick.AddListener(ShowComingSoon);

        if (playWithFriendButton != null)
            playWithFriendButton.onClick.AddListener(ShowComingSoon);
    }

    private void InitializeComingSoonPanel()
    {
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(false);
            comingSoonCanvasGroup = comingSoonPanel.GetComponent<CanvasGroup>();
            comingSoonRectTransform = comingSoonPanel.GetComponent<RectTransform>();

            conflictingAnimator = comingSoonPanel.GetComponent("GameOverPanelAnimator") as MonoBehaviour;
            if (conflictingAnimator == null)
            {
                 conflictingAnimator = comingSoonPanel.GetComponent<GameOverPanelAnimator>();
            }

            if (comingSoonCanvasGroup == null)
            {
                // Optionally add one if missing, or we'll just skip the fade effect
                comingSoonCanvasGroup = comingSoonPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void ShowComingSoon()
    {
        if (comingSoonPanel == null) return;

        if (comingSoonCoroutine != null)
        {
            StopCoroutine(comingSoonCoroutine);
        }

        comingSoonCoroutine = StartCoroutine(ComingSoonRoutine());
    }

    private IEnumerator ComingSoonRoutine()
    {
        // Force disable conflicting animator before showing
        if (conflictingAnimator != null)
        {
            conflictingAnimator.enabled = false;
        }

        comingSoonPanel.SetActive(true);
        
        // Reset position to center of screen/parent to ensure visibility
        if (comingSoonRectTransform != null)
        {
            comingSoonRectTransform.anchoredPosition = Vector2.zero;
        }

        if (comingSoonCanvasGroup != null)
        {
            comingSoonCanvasGroup.alpha = 1f;
        }

        // Wait for the display duration
        yield return new WaitForSeconds(comingSoonDisplayDuration);

        // Fade out
        if (comingSoonCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = comingSoonCanvasGroup.alpha;

            while (elapsed < comingSoonFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / comingSoonFadeDuration;
                comingSoonCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            comingSoonCanvasGroup.alpha = 0f;
        }

        comingSoonPanel.SetActive(false);
        comingSoonCoroutine = null;
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
            flagButton.onClick.RemoveListener(OnFlagButtonClick);

        if (flagCloseButton != null)
            flagCloseButton.onClick.RemoveListener(OnFlagCloseButtonClick);

        if (local2PlayersButton != null) 
            local2PlayersButton.onClick.RemoveListener(OpenLocalhostPanel);

        if (localhostCancelButton != null) 
            localhostCancelButton.onClick.RemoveListener(CloseLocalhostPanel);

        if (localhostPlayButton != null) 
            localhostPlayButton.onClick.RemoveListener(OnLocalhostPlayButtonClick);

        if (noTimeToggle != null) 
            noTimeToggle.onValueChanged.RemoveListener(OnNoTimeToggleChanged);
        
        if (hintButton != null)
            hintButton.onClick.RemoveListener(OpenHintPanel);

        if (hintCancelButton != null)
            hintCancelButton.onClick.RemoveListener(CloseHintPanel);

        if (startTutorialButton != null)
            startTutorialButton.onClick.RemoveListener(OnStartTutorialButtonClick);
        
        if (playOnlineButton != null)
            playOnlineButton.onClick.RemoveListener(ShowComingSoon);

        if (singlePlayerButton != null)
            singlePlayerButton.onClick.RemoveListener(ShowComingSoon);

        if (playWithFriendButton != null)
            playWithFriendButton.onClick.RemoveListener(ShowComingSoon);

        ClearFlagButtons();
    }

    public void RefreshPlayerName()
    {
        LoadPlayerName();
    }
}