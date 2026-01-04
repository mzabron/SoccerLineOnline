using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HandSwipeAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float moveDuration = 1.0f;

    private CanvasGroup canvasGroup;
    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    private void Awake()
    {
        // Cache components to support CanvasGroup, SpriteRenderer, or Image
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();

        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    public IEnumerator AnimateSwipe(Vector3 startPos, Vector3 endPos)
    {
        gameObject.SetActive(true);

        // 1. Setup Start Position
        transform.position = startPos;
        SetAlpha(0f);

        // 2. Move and Fade In simultaneously
        // We combine them into one loop to avoid using StartCoroutine on a just-enabled object
        float time = 0;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            
            // Move Logic
            float moveProgress = Mathf.Clamp01(time / moveDuration);
            transform.position = Vector3.Lerp(startPos, endPos, moveProgress);

            // Fade In Logic
            float fadeProgress = Mathf.Clamp01(time / fadeDuration);
            SetAlpha(Mathf.Lerp(0f, 1f, fadeProgress));

            yield return null;
        }
        
        transform.position = endPos;
        SetAlpha(1f);

        // 3. Fade Out (Sequential is fine here)
        yield return FadeTo(0f);

        gameObject.SetActive(false);
    }

    public void Hide()
    {
        StopAllCoroutines();
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = GetAlpha();
        float time = 0;
        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(alpha);
            time += Time.deltaTime;
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
        else if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
        else if (uiImage != null)
        {
            Color c = uiImage.color;
            c.a = alpha;
            uiImage.color = c;
        }
    }

    private float GetAlpha()
    {
        if (canvasGroup != null) return canvasGroup.alpha;
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (uiImage != null) return uiImage.color.a;
        return 1f;
    }
}