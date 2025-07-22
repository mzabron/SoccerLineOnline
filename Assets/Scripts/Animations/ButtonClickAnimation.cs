using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonClickAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float scaleFactor = 0.9f;
    [SerializeField] private float animationDuration = 0.1f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
        else
        {
            originalScale = transform.localScale;
        }
        
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleButton(originalScale * scaleFactor));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleButton(originalScale));
    }

    private IEnumerator ScaleButton(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}