using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class GameOverPanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType = AnimationType.SlideFromLeft;
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private bool AddFadingEffect = true;
    [SerializeField] private float delayBeforeAnimation = 0f;

    [Header("Events")]
    [SerializeField] private UnityEvent OnAnimationComplete;

    public enum AnimationType
    {
        FadeIn,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        ScaleUp
    }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    private bool isAnimating = false;
    private Coroutine currentAnimation;
    private bool hasBeenInitialized = false;

    public bool IsAnimating => isAnimating;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (AddFadingEffect)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        hasBeenInitialized = true;
    }

    void Start()
    {
        if (hasBeenInitialized)
        {
            SetToInitialAnimationState();
        }
    }

    void OnEnable()
    {
        if (hasBeenInitialized)
        {
            SetToInitialAnimationState();
        }

        if (playOnEnable && UIManager.IsAnimationsEnabled)
        {
            PlayShowAnimation();
        }
        else if (!UIManager.IsAnimationsEnabled)
        {
            SetToFinalState();
        }
    }

    void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        isAnimating = false;
    }

    private void SetToInitialAnimationState()
    {
        if (!hasBeenInitialized) return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        float screenWidth = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.width : Screen.width;
        float screenHeight = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.height : Screen.height;

        Vector3 startPosition = GetStartPosition(screenWidth, screenHeight);
        Vector3 startScale = GetStartScale();
        float startAlpha = GetStartAlpha();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localScale = startScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = startAlpha;
        }
    }

    public void PlayShowAnimation()
    {
        if (isAnimating) return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(ShowAnimationCoroutine());
    }

    public void PlayShowAnimationAfterTypewriter()
    {
        if (delayBeforeAnimation > 0f)
        {
            StartCoroutine(DelayedShowAnimation());
        }
        else
        {
            PlayShowAnimation();
        }
    }

    private IEnumerator DelayedShowAnimation()
    {
        yield return new WaitForSeconds(delayBeforeAnimation);
        PlayShowAnimation();
    }

    private IEnumerator ShowAnimationCoroutine()
    {
        isAnimating = true;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        float screenWidth = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.width : Screen.width;
        float screenHeight = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.height : Screen.height;

        Vector3 startPosition = GetStartPosition(screenWidth, screenHeight);
        Vector3 startScale = GetStartScale();
        float startAlpha = GetStartAlpha();

        Vector3 endPosition = originalPosition;
        Vector3 endScale = originalScale;
        float endAlpha = 1f;

        rectTransform.anchoredPosition = startPosition;
        rectTransform.localScale = startScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = startAlpha;
        }

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveT = animationCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, curveT);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, curveT);

            if (canvasGroup != null && ShouldAnimateAlpha())
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveT);
            }

            yield return null;
        }

        SetToFinalState();

        isAnimating = false;
        currentAnimation = null;

        OnAnimationComplete?.Invoke();
    }

    private bool ShouldAnimateAlpha()
    {
        return AddFadingEffect || animationType == AnimationType.FadeIn;
    }

    private Vector3 GetStartPosition(float screenWidth, float screenHeight)
    {
        switch (animationType)
        {
            case AnimationType.SlideFromLeft:
                return new Vector3(originalPosition.x - screenWidth, originalPosition.y, originalPosition.z);
            case AnimationType.SlideFromRight:
                return new Vector3(originalPosition.x + screenWidth, originalPosition.y, originalPosition.z);
            case AnimationType.SlideFromTop:
                return new Vector3(originalPosition.x, originalPosition.y + screenHeight, originalPosition.z);
            case AnimationType.SlideFromBottom:
                return new Vector3(originalPosition.x, originalPosition.y - screenHeight, originalPosition.z);
            default:
                return originalPosition;
        }
    }

    private Vector3 GetStartScale()
    {
        switch (animationType)
        {
            case AnimationType.ScaleUp:
                return Vector3.zero;
            default:
                return originalScale;
        }
    }

    private float GetStartAlpha()
    {
        switch (animationType)
        {
            case AnimationType.FadeIn:
                return 0f;
            default:
                return AddFadingEffect ? 0f : 1f;
        }
    }

    private void SetToFinalState()
    {
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    public void ShowImmediately()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        isAnimating = false;
        SetToFinalState();

        OnAnimationComplete?.Invoke();
    }
}