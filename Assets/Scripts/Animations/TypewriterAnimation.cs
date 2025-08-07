using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class TypewriterAnimation : MonoBehaviour
{
    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool showCursor = true;
    [SerializeField] private string cursorCharacter = "_";
    [SerializeField] private float cursorBlinkSpeed = 0.5f;
    [SerializeField] private bool hideCursorWhenComplete = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private bool playTypingSound = false;

    [Header("Events")]
    [SerializeField] private UnityEvent OnTypingComplete;

    private TMP_Text textComponent;
    private string fullText;
    private string currentText = "";
    private Coroutine typewriterCoroutine;
    private Coroutine cursorCoroutine;
    private bool isTyping = false;
    private bool isComplete = false;
    private bool cursorVisible = true;

    public bool IsTyping => isTyping;
    public bool IsComplete => isComplete;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("TypewriterAnimation: No TMP_Text component found!");
            return;
        }

        fullText = textComponent.text;
        textComponent.text = "";
    }

    void Start()
    {
        if (playOnStart && !string.IsNullOrEmpty(fullText))
        {
            StartTypewriter();
        }
    }

    public void StartTypewriter()
    {
        StartTypewriter(fullText);
    }

    public void StartTypewriter(string text)
    {
        if (textComponent == null) return;

        StopTypewriter();

        fullText = text;
        currentText = "";
        isTyping = true;
        isComplete = false;
        cursorVisible = true;

        UpdateDisplayText();

        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());

        if (showCursor)
        {
            cursorCoroutine = StartCoroutine(CursorBlinkCoroutine());
        }
    }

    public void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
            cursorCoroutine = null;
        }

        isTyping = false;
    }

    public void CompleteTypewriter()
    {
        StopTypewriter();
        currentText = fullText;
        isComplete = true;

        if (hideCursorWhenComplete)
        {
            textComponent.text = fullText;
        }
        else
        {
            UpdateDisplayText();
        }

        OnTypingComplete?.Invoke();
    }

    public void RestartTypewriter()
    {
        StartTypewriter(fullText);
    }

    private void UpdateDisplayText()
    {
        if (!showCursor)
        {
            textComponent.text = currentText;
            return;
        }

        if (isComplete && hideCursorWhenComplete)
        {
            textComponent.text = currentText;
            return;
        }

        string cursorText = cursorVisible ? cursorCharacter : $"<color=#00000000>{cursorCharacter}</color>";
        textComponent.text = currentText + cursorText;
    }

    private IEnumerator TypewriterCoroutine()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            UpdateDisplayText();

            if (playTypingSound && typingSound != null && audioSource != null && i < fullText.Length)
            {
                audioSource.PlayOneShot(typingSound);
            }

            if (i < fullText.Length)
            {
                yield return new WaitForSeconds(typeSpeed);
            }
        }

        isTyping = false;
        isComplete = true;

        if (hideCursorWhenComplete)
        {
            UpdateDisplayText();
        }

        OnTypingComplete?.Invoke();
    }

    private IEnumerator CursorBlinkCoroutine()
    {
        while (isTyping || (!hideCursorWhenComplete && isComplete))
        {
            yield return new WaitForSeconds(cursorBlinkSpeed);
            
            cursorVisible = !cursorVisible;
            UpdateDisplayText();
        }
    }

    void OnDestroy()
    {
        StopTypewriter();
    }

    [ContextMenu("Start Typewriter")]
    public void StartTypewriterFromContext()
    {
        StartTypewriter();
    }

    [ContextMenu("Complete Typewriter")]
    public void CompleteTypewriterFromContext()
    {
        CompleteTypewriter();
    }

    [ContextMenu("Restart Typewriter")]
    public void RestartTypewriterFromContext()
    {
        RestartTypewriter();
    }
}