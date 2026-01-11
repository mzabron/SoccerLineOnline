using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WelcomeUIManager : MonoBehaviour
{
    [SerializeField] private GameObject welcomeScreenCanvas;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;

    [Header("Pulse Animation Settings")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private int pulseCount = 2;
    [SerializeField] private Color pulseColor = Color.red;

    [Header("Fade Animation Settings")]
    [SerializeField] private GameObject fadeCanvas;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f; // Faster default
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Button tutorialNoButton;
    [SerializeField] private Button tutorialYesButton;
    [SerializeField] private float transitionHoldDuration = 0.2f;

    [Header("First Launch Settings")]
    [SerializeField] private bool skipFirstLaunchCheck = false;

    [Header("Main Menu References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private MenuUIManager menuUIManager;

    private Color originalColor;
    private Coroutine pulseCoroutine;
    private Coroutine fadeCoroutine;
    private bool isProcessing = false;

    private const string FIRST_LAUNCH_KEY = "HasLaunchedBefore";

    public bool IsWelcomeScreenActive => welcomeScreenCanvas != null && welcomeScreenCanvas.activeInHierarchy;
    public bool IsTutorialActive => tutorialCanvas != null && tutorialCanvas.activeInHierarchy;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Debug.Log("Default frame rate set to 60 FPS on Awake");
    }

    void Start()
    {
        InitializeWelcomeScreen();

        if (IsFirstLaunch() && !skipFirstLaunchCheck)
        {
            ShowWelcomeScreen();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private bool IsFirstLaunch()
    {
        return !PlayerPrefs.HasKey(FIRST_LAUNCH_KEY);
    }

    private void MarkAsLaunched()
    {
        PlayerPrefs.SetInt(FIRST_LAUNCH_KEY, 1);
        PlayerPrefs.Save();
    }

    private void InitializeWelcomeScreen()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }

        if (inputField != null)
        {
            inputField.characterLimit = 11;
            originalColor = inputField.colors.normalColor;
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClick);
        }

        InitializeFadeCanvas();
        InitializeTutorialCanvas();

        if (welcomeScreenCanvas != null && (!IsFirstLaunch() || skipFirstLaunchCheck))
        {
            welcomeScreenCanvas.SetActive(false);
        }
    }

    private void InitializeFadeCanvas()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);

            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = fadeCanvas.GetComponent<CanvasGroup>();
                if (fadeCanvasGroup == null)
                {
                    fadeCanvasGroup = fadeCanvas.AddComponent<CanvasGroup>();
                }
            }
        }

        if (fadeImage != null)
        {
            Color imageColor = fadeImage.color;
            imageColor.a = 0f;
            fadeImage.color = imageColor;
        }
    }

    private void InitializeTutorialCanvas()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        if (tutorialNoButton != null)
        {
            tutorialNoButton.onClick.AddListener(OnTutorialNoButtonClick);
        }

        if (tutorialYesButton != null)
        {
            tutorialYesButton.onClick.AddListener(OnTutorialYesButtonClick);
        }
    }

    public void ShowWelcomeScreen()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (welcomeScreenCanvas != null) welcomeScreenCanvas.SetActive(true);
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (welcomeScreenCanvas != null) welcomeScreenCanvas.SetActive(false);
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        if (menuUIManager != null) menuUIManager.LoadPlayerName();
    }

    private void ShowTutorial()
    {
        if (welcomeScreenCanvas != null) welcomeScreenCanvas.SetActive(false);
        if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
    }

    private void OnSubmitButtonClick()
    {
        if (isProcessing) return;

        if (inputField != null && !string.IsNullOrEmpty(inputField.text.Trim()))
        {
            PlayerPrefs.SetString("PlayerName", inputField.text.Trim());
            MarkAsLaunched();
            PlayerPrefs.Save();
            StartTutorialTransition();
        }
        else
        {
            StartPulseAnimation();
        }
    }

    private void OnTutorialNoButtonClick()
    {
        if (isProcessing) return;
        StartMainMenuTransition();
    }

    private void OnTutorialYesButtonClick()
    {
        if (isProcessing) return;

        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.ChangeSceneTo("Tutorial");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
        } 
    }

    private void StartTutorialTransition()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(TutorialTransitionCoroutine());
    }

    private void StartMainMenuTransition()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(MainMenuTransitionCoroutine());
    }

    private IEnumerator TutorialTransitionCoroutine()
    {
        isProcessing = true;
        if (submitButton != null) submitButton.interactable = false;

        // 1. Fade OUT to Black (Alpha 0 -> 1)
        yield return StartCoroutine(FadeToBlack());

        if (menuUIManager != null) menuUIManager.RefreshPlayerName();

        // 2. Switch Canvases behind the black screen
        ShowTutorial();

        // Optional hold
        if (transitionHoldDuration > 0)
            yield return new WaitForSeconds(transitionHoldDuration);

        // 3. Fade IN from Black (Alpha 1 -> 0)
        yield return StartCoroutine(FadeFromBlack());

        isProcessing = false;
        if (submitButton != null) submitButton.interactable = true;
    }

    private IEnumerator MainMenuTransitionCoroutine()
    {
        isProcessing = true;

        // 1. Fade OUT to Black
        yield return StartCoroutine(FadeToBlack());

        // 2. Switch
        ShowMainMenu();

        if (transitionHoldDuration > 0)
            yield return new WaitForSeconds(transitionHoldDuration);

        // 3. Fade IN from Black
        yield return StartCoroutine(FadeFromBlack());

        isProcessing = false;
    }

    // "Fade In" effect for the Overlay (Screen becomes dark)
    private IEnumerator FadeToBlack()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(true);
            fadeCanvas.transform.SetAsLastSibling(); // Ensure it's on top
        }

        float elapsed = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;

        // Force initial state
        SetOverlayAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float curveT = fadeCurve.Evaluate(t);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveT);
            
            SetOverlayAlpha(currentAlpha);
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    // "Fade Out" effect for the Overlay (Screen reveals)
    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvas != null) fadeCanvas.SetActive(true);

        float elapsed = 0f;
        float startAlpha = 1f;
        float targetAlpha = 0f;

        // Force initial state
        SetOverlayAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float curveT = fadeCurve.Evaluate(t);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveT);

            SetOverlayAlpha(currentAlpha);
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);

        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
        }
        
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    private void StartPulseAnimation()
    {
        if (inputField == null) return;
        if (pulseCoroutine != null) return;

        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    private IEnumerator PulseAnimation()
    {
        ColorBlock colorBlock = inputField.colors;

        for (int i = 0; i < pulseCount; i++)
        {
            float elapsed = 0f;
            while (elapsed < pulseDuration / 2f)
            {
                float t = elapsed / (pulseDuration / 2f);
                colorBlock.normalColor = Color.Lerp(originalColor, pulseColor, t);
                inputField.colors = colorBlock;

                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < pulseDuration / 2f)
            {
                float t = elapsed / (pulseDuration / 2f);
                colorBlock.normalColor = Color.Lerp(pulseColor, originalColor, t);
                inputField.colors = colorBlock;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        colorBlock.normalColor = originalColor;
        inputField.colors = colorBlock;

        pulseCoroutine = null;
    }

    void OnDestroy()
    {
        if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmitButtonClick);
        if (tutorialNoButton != null) tutorialNoButton.onClick.RemoveListener(OnTutorialNoButtonClick);
        if (tutorialYesButton != null) tutorialYesButton.onClick.RemoveListener(OnTutorialYesButtonClick);

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    [ContextMenu("Reset First Launch")]
    public void ResetFirstLaunch()
    {
        PlayerPrefs.DeleteKey(FIRST_LAUNCH_KEY);
        PlayerPrefs.Save();
        Debug.Log("First launch status reset.");
    }

    [ContextMenu("Show Welcome Screen")]
    public void ForceShowWelcomeScreen()
    {
        ShowWelcomeScreen();
    }
}