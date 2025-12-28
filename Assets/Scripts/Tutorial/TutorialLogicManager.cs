using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialLogicManager : LogicManager
{
    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text CommandText;
    [SerializeField] private HandSwipeAnimation handAnimation;
    
    private TypewriterAnimation typewriterAnimation;

    private int stepNumber = 0;
    private bool isWaitingForTypewriterComplete = false;
    private List<PulseAnimation> activeAnimations = new List<PulseAnimation>();

    private bool waitingForMove = false;
    private bool nextStepAllowed = true;
    private Node nodeBeforeMove;
    private Vector2Int savedMoveDelta;
    
    private Coroutine typewriterCoroutine;
    private Coroutine handSwipeCoroutine;

    protected override void Start()
    {
        canMove = false;
        isTutorialMode = true;
        player1Nickname = PlayerPrefs.GetString("PlayerName", "Player 1");
        base.Start();
        tutorialUI.SetActive(true);
        AdjustCameraPosition();

        if (CommandText != null)
        {
            typewriterAnimation = CommandText.GetComponent<TypewriterAnimation>();
        }
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

        if (waitingForMove)
        {
            if (currentNode != nodeBeforeMove)
            {
                savedMoveDelta = currentNode.position - nodeBeforeMove.position;
                canMove = false;
                waitingForMove = false;
                StopAllNodeAnimations();
                
                if (handSwipeCoroutine != null)
                {
                    StopCoroutine(handSwipeCoroutine);
                    if (handAnimation != null) handAnimation.Hide();
                }

                if (typewriterCoroutine != null) 
                {
                    StopCoroutine(typewriterCoroutine);
                }
                isWaitingForTypewriterComplete = false;

                if (stepNumber == 0)
                {
                    stepNumber++;
                }
                NextTutorialStep();
            }
        }

        if (isWaitingForTypewriterComplete || !canMove)
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
        else if (nextStepAllowed == true)
        {
            NextTutorialStep();
            nextStepAllowed = false;
        }
    }

    public void NextTutorialStep()
    {
        switch(stepNumber)
        {
            case 0:

                allowTap = true;
                allowSwipe = false;

                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Click on any adjacent node to move");
                HighlightNeighbors();
                nodeBeforeMove = currentNode;
                waitingForMove = true;
                break;
                
            case 1:
                nextStepAllowed = false;
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Great! Now it's your opponent's turn. Wait for his move...");
                StartCoroutine(OpponentMoveRoutine());
                break;
                
            case 2:

                allowTap = false;
                allowSwipe = true;

                ShowTypewriterText("Now try the swipe move by swiping your finger in one of the available directions.");
                
                nodeBeforeMove = currentNode;
                waitingForMove = true;

                if (handAnimation != null)
                {
                    handSwipeCoroutine = StartCoroutine(HandSwipeLoop());
                }
                break;
        }
    }

    private void ShowTypewriterText(string text)
    {
        // 1. Disable movement immediately when text starts
        canMove = false;

        if (typewriterAnimation != null)
        {
            typewriterAnimation.StartTypewriter(text);

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(WaitForTypewriterComplete());
        }
    }

    private System.Collections.IEnumerator WaitForTypewriterComplete()
    {
        while (typewriterAnimation != null && typewriterAnimation.IsTyping)
        {
            yield return null;
        }
        
        isWaitingForTypewriterComplete = false;
        stepNumber++;
        
        int finishedStepIndex = stepNumber - 1;

        if (finishedStepIndex == 0 || finishedStepIndex == 2)
        {
            canMove = true;
        }
        
        Debug.Log($"Typewriter complete. Step: {stepNumber}");
    }

    private System.Collections.IEnumerator HandSwipeLoop()
    {
        Vector3 centerPos = new Vector3(5, 0, 4);
        float swipeDistance = 3.0f;

        while (true)
        {
            List<Vector3> validSwipeDirections = new List<Vector3>();
            
            List<Node> neighbors = CheckForNeighbors();

            foreach (Node neighbor in neighbors)
            {
                Vector2Int delta = neighbor.position - currentNode.position;

                int dirIndex = GetDirectionIndex(delta);
                if (dirIndex != -1 && !currentNode.connections[dirIndex])
                {
                    validSwipeDirections.Add(new Vector3(delta.x, 0, delta.y).normalized);
                }
            }

            if (validSwipeDirections.Count > 0)
            {
                Vector3 dir = validSwipeDirections[Random.Range(0, validSwipeDirections.Count)];
                
                Vector3 startWorld = centerPos;
                Vector3 endWorld = centerPos + dir * swipeDistance;

                Vector3 startScreen = GetHandPosition(startWorld);
                Vector3 endScreen = GetHandPosition(endWorld);

                yield return handAnimation.AnimateSwipe(startScreen, endScreen);
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector3 GetHandPosition(Vector3 worldPos)
    {
        if (handAnimation.transform is RectTransform)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            screenPos.z = 0;
            return screenPos;
        }
        else
        {
            return worldPos + Vector3.up * 0.5f;
        }
    }

    private int GetDirectionIndex(Vector2Int delta)
    {
        if (delta.x == 0 && delta.y == 1) return 0;   // N
        if (delta.x == 1 && delta.y == 1) return 1;   // NE
        if (delta.x == 1 && delta.y == 0) return 2;   // E
        if (delta.x == 1 && delta.y == -1) return 3;  // SE
        if (delta.x == 0 && delta.y == -1) return 4;  // S
        if (delta.x == -1 && delta.y == -1) return 5; // SW
        if (delta.x == -1 && delta.y == 0) return 6;  // W
        if (delta.x == -1 && delta.y == 1) return 7;  // NW
        return -1;
    }

    private System.Collections.IEnumerator OpponentMoveRoutine()
    {
        while (isWaitingForTypewriterComplete)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        Vector2Int targetPos = currentNode.position + savedMoveDelta;
        Vector3 targetWorldPos = new Vector3(targetPos.x, 0, targetPos.y);
        SelectNode(targetWorldPos);

        yield return new WaitForSeconds(1.5f);
        NextTutorialStep();
    }

    private void HighlightNeighbors()
    {

        List<Node> neighbors = CheckForNeighbors();
        foreach (Node node in neighbors)
        {
            if (node.nodeObject != null)
            {
                PulseAnimation anim = node.nodeObject.GetComponent<PulseAnimation>();
                if (anim == null)
                {
                    anim = node.nodeObject.AddComponent<PulseAnimation>();
                }
                anim.StartPulsing();
                activeAnimations.Add(anim);
            }
        }
    }

    private void StopAllNodeAnimations()
    {
        foreach (var anim in activeAnimations)
        {
            if (anim != null)
            {
                anim.StopPulsing();
            }
        }
        activeAnimations.Clear();
    }
}
