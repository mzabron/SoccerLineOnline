using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialLogicManager : LogicManager
{
    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text CommandText;
    [SerializeField] private TypewriterAnimation typewriterAnimation;

    private int stepNumber = 0;
    private bool isWaitingForTypewriterComplete = false;
    
    protected override void Start()
    {
        canMove = false;
        isTutorialMode = true;
        player1Nickname = PlayerPrefs.GetString("PlayerName", "Player 1");
        base.Start();
        tutorialUI.SetActive(true);
        AdjustCameraPosition();
    }

    private void AdjustCameraPosition()
    {   
        Vector3 newCameraPosition = mainCamera.transform.position;
        newCameraPosition.z = 5.87f;
        mainCamera.transform.position = newCameraPosition;
    }

    protected override void Update()
    {
        base.Update();

        if (!canMove && !isWaitingForTypewriterComplete)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    HandleTutorialInput();
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    HandleTutorialInput();
                }
            }
#endif
        }
    }

    private void HandleTutorialInput()
    {
        if (typewriterAnimation != null && typewriterAnimation.IsTyping)
        {
            typewriterAnimation.CompleteTypewriter();
        }
        else
        {
            NextTutorialStep();
        }
    }

    public void NextTutorialStep()
    {
        switch(stepNumber)
        {
            case 0:
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Click on any adjacent node to move");
                canMove = true;
                break;
                
            case 1:
                ShowTypewriterText("Great! Now try moving to another node.");
                break;
                
            case 2:
                ShowTypewriterText("Excellent! You're learning quickly.");
                break;
        }
    }

    private void ShowTypewriterText(string text)
    {
        if (typewriterAnimation != null)
        {
            typewriterAnimation.StartTypewriter(text);
        }
    }

}
