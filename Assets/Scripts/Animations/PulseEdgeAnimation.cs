using UnityEngine;
using System.Collections;

public class PulseEdgeAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float widthMultiplier = 2.0f;
    [SerializeField] private Color highlightColor = Color.red;

    private float originalWidth;
    private Color originalColor;
    private LineRenderer lineRenderer;

    private Coroutine pulseCoroutine;
    private bool isPulsing = false;

    public void StartPulsing()
    {
        if (isPulsing) return;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        originalWidth = lineRenderer.widthMultiplier;
        originalColor = lineRenderer.material.color;

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

        if (lineRenderer != null)
        {
            lineRenderer.widthMultiplier = originalWidth;
            lineRenderer.material.color = originalColor;
        }

        isPulsing = false;
    }

    private IEnumerator PulseRoutine()
    {
        float targetWidth = originalWidth * widthMultiplier;

        while (true)
        {
            // Phase 1: Scale Up & Fade to Highlight Color
            float time = 0;
            while (time < duration)
            {
                float t = time / duration;
                if (lineRenderer != null)
                {
                    lineRenderer.widthMultiplier = Mathf.Lerp(originalWidth, targetWidth, t);
                    lineRenderer.material.color = Color.Lerp(originalColor, highlightColor, t);
                }
                time += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Scale Down & Fade back to Original Color
            time = 0;
            while (time < duration)
            {
                float t = time / duration;
                if (lineRenderer != null)
                {
                    lineRenderer.widthMultiplier = Mathf.Lerp(targetWidth, originalWidth, t);
                    lineRenderer.material.color = Color.Lerp(highlightColor, originalColor, t);
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