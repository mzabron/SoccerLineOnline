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

    private Color originalColor;
    private Coroutine pulseCoroutine;

    void Start()
    {
        if (inputField != null)
        {
            inputField.characterLimit = 11;
            // Store the original color at startup
            originalColor = inputField.colors.normalColor;
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClick);
        }
    }

    private void OnSubmitButtonClick()
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.text.Trim()))
        {
            PlayerPrefs.SetString("PlayerNickname", inputField.text.Trim());
            PlayerPrefs.Save();
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            StartPulseAnimation();
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
    }
}
