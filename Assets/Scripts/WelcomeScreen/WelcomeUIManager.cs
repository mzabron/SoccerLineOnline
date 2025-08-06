using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class WelcomeUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;

    [Header("Pulse Animation Settings")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private int pulseCount = 2;
    [SerializeField] private Color pulseColor = Color.red;

    [Header("Scene Transition Settings")]
    [SerializeField] private GameObject fadeCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image rotatingImage;
    [SerializeField] private TMP_Text constantText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Loading Animation Settings")]
    [SerializeField] private float rotationSpeed = 180f; // degrees per second
    [SerializeField] private string loadingTextFormat = "{0}%";

    private Color originalColor;
    private Coroutine pulseCoroutine;
    private Coroutine rotationCoroutine;
    private bool isTransitioning = false;

    void Start()
    {
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
    }

    private void InitializeFadeCanvas()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);
        }

        if (fadeImage != null)
        {
            Color imageColor = fadeImage.color;
            imageColor.a = 0f;
            fadeImage.color = imageColor;
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
            progressSlider.value = 0f;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
            progressText.text = string.Format(loadingTextFormat, 0);
        }

        if (rotatingImage != null)
        {
            rotatingImage.gameObject.SetActive(false);
        }

        if (constantText != null)
        {
            constantText.gameObject.SetActive(false);
        }
    }

    private void OnSubmitButtonClick()
    {
        if (isTransitioning) return;

        if (inputField != null && !string.IsNullOrEmpty(inputField.text.Trim()))
        {
            PlayerPrefs.SetString("PlayerNickname", inputField.text.Trim());
            PlayerPrefs.Save();
            StartCoroutine(LoadSceneWithFade("MainMenu"));
        }
        else
        {
            StartPulseAnimation();
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;

        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        yield return StartCoroutine(FadeIn());
        ShowLoadingElements();
        StartRotation();
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;


        while (asyncLoad.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateProgressDisplay(progress);
            yield return null;
        }


        UpdateProgressDisplay(1f);
        yield return new WaitForSeconds(0.2f);

        StopRotation();
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        HideLoadingElements();
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(true);
        }

        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = startColor;
            targetColor.a = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float curveT = fadeCurve.Evaluate(t);

                Color currentColor = Color.Lerp(startColor, targetColor, curveT);
                fadeImage.color = currentColor;

                yield return null;
            }

            fadeImage.color = targetColor;
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

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float curveT = fadeCurve.Evaluate(t);

                Color currentColor = Color.Lerp(startColor, targetColor, curveT);
                fadeImage.color = currentColor;

                yield return null;
            }

            fadeImage.color = targetColor;
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);
        }
    }

    private void ShowLoadingElements()
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = 0f;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = string.Format(loadingTextFormat, 0);
        }

        if (rotatingImage != null)
        {
            rotatingImage.gameObject.SetActive(true);
        }

        if (constantText != null)
        {
            constantText.gameObject.SetActive(true);
        }
    }

    private void HideLoadingElements()
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }

        if (rotatingImage != null)
        {
            rotatingImage.gameObject.SetActive(false);
        }

        if (constantText != null)
        {
            constantText.gameObject.SetActive(false);
        }
    }

    private void UpdateProgressDisplay(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        if (progressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100f);
            progressText.text = string.Format(loadingTextFormat, percentage);
        }
    }

    private void StartRotation()
    {
        if (rotatingImage != null)
        {
            StopRotation(); // Ensure no duplicate coroutines
            rotationCoroutine = StartCoroutine(RotateImage());
        }
    }

    private void StopRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    private IEnumerator RotateImage()
    {
        while (rotatingImage != null && isTransitioning)
        {
            rotatingImage.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            yield return null;
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

    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitButtonClick);
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
    }
}
