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
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Button tutorialNoButton;
    [SerializeField] private Button tutorialYesButton;
    [SerializeField] private float transitionHoldDuration = 0.5f;

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
                    Debug.Log("CanvasGroup component added automatically to fadeCanvas");
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
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }

        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(true);
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        Debug.Log("Welcome screen activated");
    }

    public void ShowMainMenu()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }

        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(false);
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        if (menuUIManager != null)
        {
            menuUIManager.LoadPlayerName();
        }

        Debug.Log("Main menu activated");
    }

    private void ShowTutorial()
    {
        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(false);
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(true);
        }

        Debug.Log("Tutorial canvas activated");
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

        Debug.Log("Tutorial 'No' button clicked - transitioning to main menu");
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
            Debug.LogWarning("SceneLoader not found. Loading Tutorial scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
        }
    }

    private void StartTutorialTransition()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(TutorialTransitionCoroutine());
    }

    private void StartMainMenuTransition()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(MainMenuTransitionCoroutine());
    }

    private IEnumerator TutorialTransitionCoroutine()
    {
        isProcessing = true;

        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        if (menuUIManager != null)
        {
            menuUIManager.LoadPlayerName();
        }

        yield return StartCoroutine(FadeIn());

        ShowTutorial();

        yield return new WaitForSeconds(transitionHoldDuration);
        yield return StartCoroutine(FadeOut());

        isProcessing = false;

        Debug.Log("Tutorial transition completed");
    }

    private IEnumerator MainMenuTransitionCoroutine()
    {
        isProcessing = true;

        yield return StartCoroutine(FadeIn());
        ShowMainMenu();

        yield return new WaitForSeconds(transitionHoldDuration);
        yield return StartCoroutine(FadeOut());

        isProcessing = false;

        Debug.Log("Main menu transition completed");
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(true);
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }
        }

        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = startColor;
            targetColor.a = 1f;

            float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
            float targetAlpha = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float curveT = fadeCurve.Evaluate(t);

                Color currentColor = Color.Lerp(startColor, targetColor, curveT);
                fadeImage.color = currentColor;
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveT);
                }

                yield return null;
            }

            fadeImage.color = targetColor;
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = targetAlpha;
            }
        }
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = startColor;
            targetColor.a = 0f;

            float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 1f;
            float targetAlpha = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float curveT = fadeCurve.Evaluate(t);

                Color currentColor = Color.Lerp(startColor, targetColor, curveT);
                fadeImage.color = currentColor;
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveT);
                }

                yield return null;
            }

            fadeImage.color = targetColor;
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = targetAlpha;
            }
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);
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
        Debug.Log("WelcomeUIManager: OnDestroy called");

        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitButtonClick);
        }

        if (tutorialNoButton != null)
        {
            tutorialNoButton.onClick.RemoveListener(OnTutorialNoButtonClick);
        }

        if (tutorialYesButton != null)
        {
            tutorialYesButton.onClick.RemoveListener(OnTutorialYesButtonClick);
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }

    [ContextMenu("Reset First Launch")]
    public void ResetFirstLaunch()
    {
        PlayerPrefs.DeleteKey(FIRST_LAUNCH_KEY);
        PlayerPrefs.Save();
        Debug.Log("First launch status reset. Welcome screen will show on next launch.");
    }

    [ContextMenu("Show Welcome Screen")]
    public void ForceShowWelcomeScreen()
    {
        ShowWelcomeScreen();
    }

    [ContextMenu("Show Main Menu")]
    public void ForceShowMainMenu()
    {
        ShowMainMenu();
    }

    [ContextMenu("Show Tutorial")]
    public void ForceShowTutorial()
    {
        ShowTutorial();
    }
}