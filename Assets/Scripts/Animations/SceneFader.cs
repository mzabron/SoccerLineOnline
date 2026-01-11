using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;
    
    public Coroutine changeAlphaCoroutine { get; private set; }

    private void Start()
    {
        if (canvasGroup != null)
        {
            Image bgImage = canvasGroup.GetComponent<Image>();
            if (bgImage != null)
            {
                Color fixedColor = bgImage.color;
                fixedColor.a = 1f; // Force full opacity
                bgImage.color = fixedColor;
            }


            Canvas parentCanvas = canvasGroup.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                parentCanvas.sortingOrder = 999; 
            }

            canvasGroup.alpha = 1f;
            canvasGroup.gameObject.SetActive(true);
        }
        DoFadeIn();
    }

    public void DoFadeIn()
    {
        FadeEffect(0f);
    }

    public void DoFadeOut()
    {
        FadeEffect(1f);
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
        if (canvasGroup == null)
        {
            Debug.LogError("SceneFader: No CanvasGroup assigned!");
            yield break;
        }

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        float timePassed = 0;
        float startAlpha = canvasGroup.alpha;

        while (timePassed < fadeDuration)
        {
            timePassed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timePassed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);
        }
    }
}
