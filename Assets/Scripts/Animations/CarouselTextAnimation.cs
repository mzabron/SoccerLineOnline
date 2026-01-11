using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class CarouselTextAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float slideOffset = 150f;
    
    private TMP_Text targetText;
    private Vector3 initialPosition;
    private bool isInitialized = false;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
        // Fallback if component is on a parent but text is on child, etc.
        if (targetText == null)
        {
            targetText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Initialize()
    {
        if (!isInitialized && targetText != null)
        {
            initialPosition = targetText.rectTransform.localPosition;
            isInitialized = true;
        }
    }

    /// <summary>
    /// Animates the text sliding out and back in. 
    /// </summary>
    /// <param name="direction">Positive for 'Next', Negative for 'Previous'. Affects slide direction.</param>
    /// <param name="onContentUpdate">Action to perform when text is invisible (e.g., updating the string).</param>
    public void AnimateChange(int direction, Action onContentUpdate)
    {
        if (targetText == null)
        {
            onContentUpdate?.Invoke();
            return;
        }

        Initialize();

        // Stop any running animation and reset immediately
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            targetText.rectTransform.localPosition = initialPosition;
            targetText.alpha = 1f;
        }

        activeCoroutine = StartCoroutine(AnimateRoutine(direction, onContentUpdate));
    }

    private IEnumerator AnimateRoutine(int direction, Action onContentUpdate)
    {
        RectTransform txtRect = targetText.rectTransform;

        // Visual direction logic:
        // Clicking Right Arrow (direction > 0) -> Text slides out to Left (visual -1)
        float visualDir = direction > 0 ? -1f : 1f;

        // Phase 1: Slide Out
        float elapsed = 0f;
        Vector3 startPos = txtRect.localPosition;
        Vector3 targetOut = initialPosition + new Vector3(visualDir * slideOffset, 0, 0);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            
            // EaseIn: t*t
            txtRect.localPosition = Vector3.Lerp(startPos, targetOut, t * t); 
            targetText.alpha = 1f - t;
            yield return null;
        }

        // --- MIDPOINT: Text is invisible ---
        onContentUpdate?.Invoke();
        
        // Phase 2: Slide In - Start from opposite side
        Vector3 startIn = initialPosition + new Vector3(-visualDir * slideOffset, 0, 0);
        txtRect.localPosition = startIn;
        
        elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            
            // EaseOut: 1-(1-t)^2
            float easeOut = 1f - (1f - t) * (1f - t);
            
            txtRect.localPosition = Vector3.Lerp(startIn, initialPosition, easeOut);
            targetText.alpha = t;
            yield return null;
        }

        // Finalize
        txtRect.localPosition = initialPosition;
        targetText.alpha = 1f;
        activeCoroutine = null;
    }

    private void OnDisable()
    {
        // Reset state if the object invalidates (e.g. panel closes)
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        
        if (isInitialized && targetText != null)
        {
            targetText.rectTransform.localPosition = initialPosition;
            targetText.alpha = 1f;
        }
    }
}
