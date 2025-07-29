using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingHintAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatDistance = 10f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private bool startAnimationOnEnable = true;

    [Header("Trigger Settings")]
    [SerializeField] private Button triggerButton; // Button to trigger hint dismissal
    [SerializeField] private string playerPrefKey = "FlagHintDismissed";
    [SerializeField] private bool dismissPermanently = true;

    private Vector3 originalPosition;
    private bool isAnimating = false;
    private bool hasBeenDismissed = false;
    private Coroutine floatingCoroutine;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        originalPosition = transform.localPosition;

        if (dismissPermanently)
        {
            hasBeenDismissed = PlayerPrefs.GetInt(playerPrefKey, 0) == 1;
        }

        if (hasBeenDismissed)
        {
            gameObject.SetActive(false);
            return;
        }

        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(OnTriggerButtonClick);
        }

        if (startAnimationOnEnable)
        {
            StartFloatingAnimation();
        }
    }

    void OnEnable()
    {
        if (originalPosition != Vector3.zero)
        {
            transform.localPosition = originalPosition;
        }
        else
        {
            originalPosition = transform.localPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (!hasBeenDismissed && startAnimationOnEnable)
        {
            StartFloatingAnimation();
        }
    }

    void OnDisable()
    {
        StopFloatingAnimation();
    }

    public void StartFloatingAnimation()
    {
        if (hasBeenDismissed || isAnimating) return;

        isAnimating = true;
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
        }
        floatingCoroutine = StartCoroutine(FloatingAnimation());
    }

    public void StopFloatingAnimation()
    {
        isAnimating = false;
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
            floatingCoroutine = null;
        }
    }

    public void DismissHint()
    {
        if (hasBeenDismissed) return;

        hasBeenDismissed = true;
        isAnimating = false;

        if (dismissPermanently)
        {
            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }

        StopFloatingAnimation();
        StartCoroutine(FadeOutAnimation());
    }

    public void ResetHint()
    {
        hasBeenDismissed = false;

        if (dismissPermanently)
        {
            PlayerPrefs.SetInt(playerPrefKey, 0);
            PlayerPrefs.Save();
        }

        gameObject.SetActive(true);
        transform.localPosition = originalPosition;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        StartFloatingAnimation();
    }

    private void OnTriggerButtonClick()
    {
        DismissHint();
    }

    private IEnumerator FloatingAnimation()
    {
        while (isAnimating && !hasBeenDismissed)
        {
            float offset = Mathf.Sin(Time.time * floatSpeed) * floatDistance;
            transform.localPosition = originalPosition + Vector3.up * offset;

            yield return null;
        }
    }

    private IEnumerator FadeOutAnimation()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(OnTriggerButtonClick);
        }

        StopFloatingAnimation();
    }

    void OnValidate()
    {
        floatDistance = Mathf.Abs(floatDistance);
        floatSpeed = Mathf.Max(0.1f, floatSpeed);
        fadeOutDuration = Mathf.Max(0.1f, fadeOutDuration);
    }
}