using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuUIManager : SettingsUI
{
    [Header("Menu Specific UI")]
    private bool isLoggedIn = true;

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

    private List<GameObject> instantiatedFlagButtons = new List<GameObject>();
    private GridLayoutGroup gridLayout;
    private RectTransform gridRectTransform;
    private ScrollRect flagScrollRect;
    private Vector2 lastFlagButtonSize;
    private Vector2 lastFlagSpacing;
    private RectOffset lastGridPadding;
    private float lastPanelWidth;

    public static bool IsFlagSelectionOpen { get; private set; } = false;


    void Awake()
    {
        // TEMPORARY: Clear PlayerPrefs in Awake to ensure it happens before other scripts' Start()
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    protected override void Start()
    {
        base.Start();
        InitializeFlagSelection();
        LoadSelectedFlag();
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
                
                Debug.Log($"Set flag name text to: {FormatFlagName(flagSprite.name)} with scaled font size: {flagNameText.fontSize}");
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

        ClearFlagButtons();
    }
}