using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;
    public Coroutine changeAlphaCoroutine { get; private set; }

    private void Start()
    {
        DoFadeIn();
    }

    public void DoFadeIn()
    {
        FadeEffect(0f);
    }

    public void DoFadeOut()
    {
        FadeEffect(1);
    }

    private void FadeEffect(float targetAlpha)
    {
        if (changeAlphaCoroutine != null)
        {
            StopCoroutine(changeAlphaCoroutine);
        }
        changeAlphaCoroutine = StartCoroutine(ChangeAlpha(targetAlpha));
    }

    private IEnumerator ChangeAlpha(float targetAlpha)
    {
        float timePassed = 0;
        float startAlpha = canvasGroup.alpha;

        while (timePassed < fadeDuration)
        {
            timePassed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timePassed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
