using UnityEngine;
using System.Collections;

public class PulseAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private Color highlightColor = Color.orange;

    private Vector3 originalScale;
    private Color originalColor;
    private Renderer objectRenderer;

    private Coroutine pulseCoroutine;
    private bool isPulsing = false;

    public void StartPulsing()
    {
        if (isPulsing) return;

        originalScale = transform.localScale;

        // Get the renderer and store the original color
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        isPulsing = true;
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    public void StopPulsing()
    {
        if (!isPulsing) return;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Reset scale
        transform.localScale = originalScale;

        // Reset color
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }

        isPulsing = false;
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 targetScale = originalScale * scaleMultiplier;

        while (true)
        {
            // Phase 1: Scale Up & Fade to Highlight Color
            float time = 0;
            while (time < duration)
            {
                float t = time / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                
                if (objectRenderer != null)
                {
                    objectRenderer.material.color = Color.Lerp(originalColor, highlightColor, t);
                }

                time += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Scale Down & Fade back to Original Color
            time = 0;
            while (time < duration)
            {
                float t = time / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);

                if (objectRenderer != null)
                {
                    objectRenderer.material.color = Color.Lerp(highlightColor, originalColor, t);
                }

                time += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void OnDisable()
    {
        StopPulsing();
    }
}