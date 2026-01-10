using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TutorialLogicManager : LogicManager
{
    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text CommandText;
    [SerializeField] private HandSwipeAnimation handAnimation;
    [SerializeField] private Image blackScreenPanel;
    [SerializeField] private GameObject completePanel;

    private TypewriterAnimation typewriterAnimation;

    private int stepNumber = 0;
    private bool isWaitingForTypewriterComplete = false;
    private List<PulseAnimation> activeAnimations = new List<PulseAnimation>();
    private List<PulseEdgeAnimation> activeEdgeAnimations = new List<PulseEdgeAnimation>();

    private bool waitingForMove = false;
    private bool nextStepAllowed = true;
    private Node nodeBeforeMove;
    private Vector2Int savedMoveDelta;

    private Coroutine typewriterCoroutine;
    private Coroutine handSwipeCoroutine;

    private static int tutorialCheckpoint = -1;
    private int step8MoveCount = 0;


    private static Vector2Int step5Target = new Vector2Int(0, 5);
    private static Vector2Int step6Target = new Vector2Int(1, 6);
    private static Vector2Int step7Target = new Vector2Int(2, 6);

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

            if (tutorialCheckpoint == 4)
            {
                stepNumber = 4;
                tutorialCheckpoint = -1;
                nextStepAllowed = false;
                StartCoroutine(SetupJumpScenarioPostRestart());
            }
            else if (tutorialCheckpoint == 8)
            {
                stepNumber = 8;
                tutorialCheckpoint = -1;
                nextStepAllowed = false;
                StartCoroutine(SetupStep8ScenarioPostRestart());
            }
            else if (tutorialCheckpoint == 10)
            {
                stepNumber = 10;
                tutorialCheckpoint = -1;
                nextStepAllowed = false;
                StartCoroutine(SetupCase10ScenarioPostRestart());
            }
            else if (tutorialCheckpoint == 12)
            {
                stepNumber = 12;
                tutorialCheckpoint = -1;
                nextStepAllowed = false;
                StartCoroutine(SetupCase12ScenarioPostRestart());
            }
            // Add Start checkpoint for Case 14
            else if (tutorialCheckpoint == 14)
            {
                stepNumber = 14;
                tutorialCheckpoint = -1;
                nextStepAllowed = false;
                StartCoroutine(SetupCase14ScenarioPostRestart());
            }
            else
            {
                stepNumber = -1;
                ShowTypewriterText($"Hi {player1Nickname}, you're going to learn basics. Click anywhere to continue.");
            }
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

        // If completion UI is up, don't intercept clicks/taps (let UI buttons work)
        if (completePanel != null && completePanel.activeInHierarchy)
            return;

        bool isGoalScored = soccer != null && soccer.transform.position.z > 10.2f;

        // Step 12 Win Logic
        if (stepNumber == 12 && isGoalScored)
        {
            stepNumber = 13;
            NextTutorialStep();
        }

        // --- STEP 14 WIN LOGIC ---
        if (stepNumber == 14 && isGoalScored && canMove)
        {
            stepNumber = 15;
            StopAllNodeAnimations();
            StopAllEdgeAnimations();
            waitingForMove = false;
            canMove = false;
            NextTutorialStep();
            return;
        }

        if (waitingForMove)
        {
            if (currentNode != nodeBeforeMove)
            {
                // Step 5 Logic (Boundary Bounce)
                if (stepNumber == 5)
                {
                    if (IsBoundaryNode(currentNode))
                    {
                        step5Target = currentNode.position;
                        nodeBeforeMove = currentNode;
                        StopAllNodeAnimations();
                        nextStepAllowed = false;
                        ShowTypewriterText("Continue your turn. Note that you can't cross drawn or highlighted boundary lines.");
                        HighlightBoundaryEdges();
                        return;
                    }
                    else
                    {
                        canMove = false;
                        waitingForMove = false;
                        StopAllNodeAnimations();
                        StartCoroutine(ResetJumpScenario(4));
                        return;
                    }
                }

                // Step 8 Logic (Line Bounce)
                if (stepNumber == 8)
                {
                    step8MoveCount++;
                    int connections = 0;
                    foreach (bool c in currentNode.connections) if (c) connections++;
                    bool canBounce = connections > 1 || IsBoundaryNode(currentNode);

                    if (canBounce)
                    {
                        nodeBeforeMove = currentNode;
                        StopAllNodeAnimations();
                        ShowTypewriterText("Continue your turn", false);
                        return;
                    }
                    else
                    {
                        if (step8MoveCount == 1)
                        {
                            canMove = false;
                            waitingForMove = false;
                            StopAllNodeAnimations();
                            StartCoroutine(ResetJumpScenario(8));
                            return;
                        }
                        else
                        {
                            StopAllNodeAnimations();
                            stepNumber = 9;
                            NextTutorialStep();
                            return;
                        }
                    }
                }

                if (stepNumber == 12)
                {
                    canMove = false;
                    waitingForMove = false;
                    StopAllNodeAnimations();
                    
                    if (!isGoalScored)
                    {
                        StartCoroutine(CheckStep12Result());
                        return;
                    }
                }

                // --- STEP 14 LOST TURN LOGIC ---
                if (stepNumber == 14)
                {
                    // If currentPlayer switched to 2, Player 1 lost the turn -> Restart
                    if (currentPlayer != 1)
                    {
                        canMove = false;
                        waitingForMove = false;
                        StopAllNodeAnimations();
                        StartCoroutine(SetupCase14Scenario());
                        return;
                    }

                    // If still Player 1, they made a valid bounce. 
                    // Update the node reference and let them continue playing.
                    nodeBeforeMove = currentNode;
                    StopAllNodeAnimations();
                    return; 
                }

                savedMoveDelta = currentNode.position - nodeBeforeMove.position;

                // Capture Step 6 move
                if (stepNumber == 6)
                {
                    step6Target = currentNode.position;
                }

                canMove = false;
                waitingForMove = false;
                StopAllNodeAnimations();
                StopAllEdgeAnimations();

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

    private void HighlightBoundaryEdges()
    {
        // Horizontal edges (Bottom y=0 and Top y=height-1)
        for (int x = 0; x < width - 1; x++)
        {
            // Bottom edge: Skip goal segments (3,0)-(4,0) and (4,0)-(5,0)
            if (x != 3 && x != 4)
            {
                HighlightEdge(new Vector2Int(x, 0), new Vector2Int(x + 1, 0));
            }

            // Top edge: Skip goal segments (3,10)-(4,10) and (4,10)-(5,10)
            if (x != 3 && x != 4)
            {
                HighlightEdge(new Vector2Int(x, height - 1), new Vector2Int(x + 1, height - 1));
            }
        }

        // Vertical edges (Left x=0 and Right x=width-1)
        for (int y = 0; y < height - 1; y++)
        {
            HighlightEdge(new Vector2Int(0, y), new Vector2Int(0, y + 1));
            HighlightEdge(new Vector2Int(width - 1, y), new Vector2Int(width - 1, y + 1));
        }
    }

    private System.Collections.IEnumerator CheckStep12Result()
    {
        yield return null;
        
        if (stepNumber == 12 && !isGameOver)
        {
            StartCoroutine(ResetJumpScenario(12));
        }
    }

    private bool IsBoundaryNode(Node node)
    {
        int x = node.position.x;
        int y = node.position.y;

        bool isBoundary = (x == 0 || x == width - 1 || y == 0 || y == height - 1);

        // Exclude corners
        if ((x == 0 && y == 0) ||
            (x == 0 && y == height - 1) ||
            (x == width - 1 && y == 0) ||
            (x == width - 1 && y == height - 1))
            return false;

        // Exclude goals
        if ((x == 4 && y == 0) || (x == 4 && y == 10))
            return false;

        return isBoundary;
    }

    private System.Collections.IEnumerator ResetJumpScenario(int checkpoint = 4)
    {
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(blackScreenPanel, 0f, 1f, 0.5f));
        }

        tutorialCheckpoint = checkpoint;
        RestartGame();
    }

    private void HandleTutorialInput()
    {
        if (typewriterAnimation != null && typewriterAnimation.IsTyping)
        {
            typewriterAnimation.CompleteTypewriter();
        }
        else if (isWaitingForTypewriterComplete)
        {
            return;
        }
        else if (nextStepAllowed == true)
        {
            NextTutorialStep();
            nextStepAllowed = false;
        }
    }

    public void NextTutorialStep()
    {
        switch (stepNumber)
        {
            case 0:

                allowTap = true;
                allowSwipe = false;

                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Click on any adjacent node to move.");
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
                // Move hand animation start BEFORE text to ensure it initializes concurrent with text start
                if (handAnimation != null)
                {
                    if (handSwipeCoroutine != null) StopCoroutine(handSwipeCoroutine);
                    handSwipeCoroutine = StartCoroutine(HandSwipeLoop());
                }

                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Now try the swipe move by swiping your finger in one of the available directions.");

                nodeBeforeMove = currentNode;
                waitingForMove = true;
                break;

            case 3:
                allowSwipe = true;
                allowTap = true;
                nextStepAllowed = true;
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Excellent! Now you're going to learn about jumps. Tap anywhere to continue.");
                break;

            case 4:
                StartCoroutine(SetupJumpScenario());
                break;

            //case 5 is implemented in Update() method when waiting for move

            case 6:
                nextStepAllowed = true;
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Good job! You can also bounce off existing lines. Tap anywhere to end your turn.");
                break;

            case 7:
                nextStepAllowed = false;
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Waiting for oponnent's move...");
                StartCoroutine(OpponentMoveRoutine());
                break;

            case 8:
                step8MoveCount = 0;
                nextStepAllowed = false;
                isWaitingForTypewriterComplete = true;
                HighlightStep8Lines();
                ShowTypewriterText("Now try bouncing off one of the already drawn lines.", false);
                waitingForMove = true;
                nodeBeforeMove = currentNode;
                break;

            case 9:
                nextStepAllowed = true;
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Good job! However, sometimes you may end up with no valid moves. Tap anywhere to see an example.");
                break;

            case 10:
                StartCoroutine(SetupCase10Scenario());
                break;

            case 11:
                // Move ball to (8,5)
                if (currentNode.position != new Vector2Int(8, 5))
                {
                    SelectNode(new Vector3(8, 0, 5));
                }

                // Immediately reset game over trigger so the visual UI does not appear
                isGameOver = false;

                // Highlight blocking edges
                HighlightEdge(new Vector2Int(8, 5), new Vector2Int(7, 6));
                HighlightEdge(new Vector2Int(8, 5), new Vector2Int(7, 5));
                HighlightEdge(new Vector2Int(8, 5), new Vector2Int(7, 4));

                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("As you can see, there's no way out. Getting stuck results in losing the game. Tap anywhere to continue.");
                nextStepAllowed = true;
                break;

            case 12:
                StartCoroutine(SetupCase12Scenario());
                break;
            
            case 13:
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Nice! Click anywhere to take the last step.");
                nextStepAllowed = true;
                break;

            case 14:
                StartCoroutine(SetupCase14Scenario());
                break;

            case 15:
                isWaitingForTypewriterComplete = true;
                ShowTypewriterText("Congrats!", false);
                if (completePanel != null)
                {
                    completePanel.gameObject.SetActive(true);
                }
                nextStepAllowed = false;
                waitingForMove = false;
                canMove = false;
                break;
        }
    }

    private System.Collections.IEnumerator SetupCase14Scenario()
    {
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(blackScreenPanel, 0f, 1f, 0.5f));
        }
        tutorialCheckpoint = 14;
        RestartGame();
    }

    private System.Collections.IEnumerator SetupCase14ScenarioPostRestart()
    {
        allowSwipe = false;
        allowTap = false;

        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            Color c = blackScreenPanel.color;
            c.a = 1f;
            blackScreenPanel.color = c;
        }

        isTutorialMode = true;
        yield return new WaitForSeconds(0.1f);

        
        SelectNode(new Vector3(4, 0, 6)); yield return new WaitForSeconds(0.02f); // P1
        SelectNode(new Vector3(4, 0, 7)); yield return new WaitForSeconds(0.02f); // P2
        SelectNode(new Vector3(4, 0, 8)); yield return new WaitForSeconds(0.02f); // P1
        SelectNode(new Vector3(4, 0, 9)); yield return new WaitForSeconds(0.02f); // P2
        SelectNode(new Vector3(4, 0, 10)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(3, 0, 9)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(4, 0, 8)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(3, 0, 8)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(4, 0, 7)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(3, 0, 6)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(4, 0, 6)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(3, 0, 7)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(2, 0, 6)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(1, 0, 7)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(1, 0, 6)); yield return new WaitForSeconds(0.02f);
        SelectNode(new Vector3(1, 0, 5)); yield return new WaitForSeconds(0.02f);

        currentPlayer = 1;

        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadePanel(blackScreenPanel, 1f, 0f, 0.5f));
            blackScreenPanel.gameObject.SetActive(false);
        }

        currentPlayer = 1;
        allowSwipe = true;
        allowTap = true;
        nextStepAllowed = false;
        isWaitingForTypewriterComplete = true;

        ShowTypewriterText("Use everything you've learned so far to win the game on your turn!", false);

        canMove = true;
        nodeBeforeMove = currentNode;
        waitingForMove = true;
    }

    private System.Collections.IEnumerator SetupCase12Scenario()
    { 
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(blackScreenPanel, 0f, 1f, 0.5f));
        }
        tutorialCheckpoint = 12;
        RestartGame();
    }

    private System.Collections.IEnumerator SetupCase10Scenario()
    {
        // Fade Out
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(blackScreenPanel, 0f, 1f, 0.5f));
        }

        tutorialCheckpoint = 10;
        RestartGame();
    }

    private System.Collections.IEnumerator SetupCase12ScenarioPostRestart()
    {
        allowSwipe = false;
        allowTap = false;


        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            Color c = blackScreenPanel.color;
            c.a = 1f;
            blackScreenPanel.color = c;
        }

        isTutorialMode = true;
        yield return new WaitForSeconds(0.1f);

        SelectNode(new Vector3(4, 0, 6)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(4, 0, 7)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(4, 0, 8)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(4, 0, 9)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(4, 0, 10)); yield return new WaitForSeconds(0.1f);

        currentPlayer = 1;

        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadePanel(blackScreenPanel, 1f, 0f, 0.5f));
            blackScreenPanel.gameObject.SetActive(false);
        }

        currentPlayer = 1;
        allowSwipe = true;
        allowTap = true;
        nextStepAllowed = false;
        isWaitingForTypewriterComplete = true;

        ShowTypewriterText("Ready to score? Swipe toward the opponent's goal or tap on it to win the game.", false);

        canMove = true;
        nodeBeforeMove = currentNode;
        waitingForMove = true;
    }

    private System.Collections.IEnumerator SetupCase10ScenarioPostRestart()
    {
        allowSwipe = false;
        allowTap = false;

        
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            Color c = blackScreenPanel.color;
            c.a = 1f;
            blackScreenPanel.color = c;
        }

        isTutorialMode = true;
        
        yield return new WaitForSeconds(0.1f);

        // Execute scenario moves
        // LogicManager.SelectNode allows moving even if canMove is false, as canMove checks are in Update().
        SelectNode(new Vector3(5, 0, 5)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(6, 0, 5)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(7, 0, 6)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(8, 0, 5)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(7, 0, 4)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(7, 0, 5)); yield return new WaitForSeconds(0.1f);

        currentPlayer = 1;

        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadePanel(blackScreenPanel, 1f, 0f, 0.5f));
            blackScreenPanel.gameObject.SetActive(false);
        }

        allowSwipe = true;
        allowTap = true;
        nextStepAllowed = true;
        isWaitingForTypewriterComplete = true;
        ShowTypewriterText("Here, after moving to the right, you'll be stuck. Tap anywhere to continue.");
        
        nodeBeforeMove = currentNode;
        waitingForMove = true;
    }

    private System.Collections.IEnumerator SetupJumpScenario()
    {
        // Fade Out
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(blackScreenPanel, 0f, 1f, 0.5f));
        }

        tutorialCheckpoint = 4;
        RestartGame();
    }

    private System.Collections.IEnumerator SetupJumpScenarioPostRestart()
    {
        allowSwipe = false;
        allowTap = false;


        // Ensure screen is black initially after reload
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            Color c = blackScreenPanel.color;
            c.a = 1f;
            blackScreenPanel.color = c;
        }

        isTutorialMode = true;
        
        yield return new WaitForSeconds(0.1f);

        SelectNode(new Vector3(3, 0, 5));
        yield return new WaitForSeconds(0.2f);
        
        SelectNode(new Vector3(2, 0, 5));
        yield return new WaitForSeconds(0.2f);

        SelectNode(new Vector3(1, 0, 5));
        yield return new WaitForSeconds(0.2f);

        currentPlayer = 1;

        // Fade In
        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadePanel(blackScreenPanel, 1f, 0f, 0.5f));
            blackScreenPanel.gameObject.SetActive(false);
        }

        allowSwipe = true;
        allowTap = true;
        nextStepAllowed = true;
        isWaitingForTypewriterComplete = true;
        
        HighlightBoundaryNodes();

        ShowTypewriterText("Landing on one of the highlighted boundary node requires a bounce and an extra move. Try it!");

        nodeBeforeMove = currentNode;
        waitingForMove = true;
    }

    private System.Collections.IEnumerator SetupStep8ScenarioPostRestart()
    {
        // Ensure screen is black initially after reload
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            Color c = blackScreenPanel.color;
            c.a = 1f;
            blackScreenPanel.color = c;
        }

        isTutorialMode = true;
        yield return new WaitForSeconds(0.1f);

        // Replay moves to restore state up to Step 8

        
        // Step 4 moves (Setup)
        SelectNode(new Vector3(3, 0, 5)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(2, 0, 5)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(1, 0, 5)); yield return new WaitForSeconds(0.1f);
        
        // Step 5 & 6 moves (Replay player's boundary bounce)
        SelectNode(new Vector3(step5Target.x, 0, step5Target.y)); yield return new WaitForSeconds(0.1f);
        SelectNode(new Vector3(step6Target.x, 0, step6Target.y)); yield return new WaitForSeconds(0.1f);
        
        // Step 7 Opponent move (Replay opponent's move)
        SelectNode(new Vector3(step7Target.x, 0, step7Target.y)); yield return new WaitForSeconds(0.1f);

        currentPlayer = 1;

        // Fade In
        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadePanel(blackScreenPanel, 1f, 0f, 0.5f));
            blackScreenPanel.gameObject.SetActive(false);
        }

        // Initialize Step 8
        step8MoveCount = 0;
        nextStepAllowed = false;
        isWaitingForTypewriterComplete = true;
        HighlightStep8Lines();
        ShowTypewriterText("Now try bouncing off one of the already drawn lines.", false);
        waitingForMove = true; 
        nodeBeforeMove = currentNode;
    }

    private void HighlightStep8Lines()
    {
        List<Vector2Int> nodesToHighlight = new List<Vector2Int> 
        { 
            new Vector2Int(1, 5), 
            new Vector2Int(2, 5), 
            new Vector2Int(3, 5) 
        };

        foreach (Vector2Int pos in nodesToHighlight)
        {
            Node node = board[pos.x, pos.y];
            if (node != null && node.nodeObject != null)
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

    private void HighlightBoundaryNodes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isBoundary = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                if (!isBoundary) continue;

                // Exclude corners
                if ((x == 0 && y == 0) || 
                    (x == 0 && y == height - 1) || 
                    (x == width - 1 && y == 0) || 
                    (x == width - 1 && y == height - 1))
                    continue;

                // Exclude mid-goal nodes (4,0) and (4,10)
                if ((x == 4 && y == 0) || (x == 4 && y == 10))
                    continue;

                Node node = board[x, y];
                if (node != null && node.nodeObject != null)
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
    }

    private System.Collections.IEnumerator FadePanel(Image panel, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color startColor = panel.color;
        startColor.a = startAlpha;
        panel.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            Color newColor = panel.color;
            newColor.a = alpha;
            panel.color = newColor;
            yield return null;
        }

        Color finalColor = panel.color;
        finalColor.a = endAlpha;
        panel.color = finalColor;
    }

    private void ShowTypewriterText(string text, bool advanceStep = true)
    {
        // 1. Disable movement immediately when text starts
        canMove = false;

        if (typewriterAnimation != null)
        {
            typewriterAnimation.StartTypewriter(text);

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(WaitForTypewriterComplete(advanceStep));
        }
    }

    private System.Collections.IEnumerator WaitForTypewriterComplete(bool advanceStep)
    {
        while (typewriterAnimation != null && typewriterAnimation.IsTyping)
        {
            yield return null;
        }
        
        isWaitingForTypewriterComplete = false;
        
        if (advanceStep)
        {
            stepNumber++;
        }
        
        bool shouldEnableMove = false;
        int checkStep = advanceStep ? stepNumber - 1 : stepNumber;

        if (checkStep == 0 || checkStep == 2 || checkStep == 4 || checkStep == 5 || checkStep == 8)
        {
            shouldEnableMove = true;
        }
        
        if (shouldEnableMove)
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

        Vector3 targetWorldPos;

        // Step 7 logic (stepNumber is 8 here because WaitForTypewriterComplete increments it)
        if (stepNumber == 8)
        {
            Vector2Int currentPos = currentNode.position;
            Vector2Int targetPos = currentPos;

            if ((currentPos.x == 1 && currentPos.y == 6) || (currentPos.x == 1 && currentPos.y == 7))
            {
                targetPos = new Vector2Int(2, 6);
            }
            else if ((currentPos.x == 1 && currentPos.y == 4) || (currentPos.x == 1 && currentPos.y == 3))
            {
                targetPos = new Vector2Int(2, 4);
            }
            
            step7Target = targetPos; // Capture Step 7 move
            targetWorldPos = new Vector3(targetPos.x, 0, targetPos.y);
        }
        else
        {
            // Default logic for Step 1
            Vector2Int targetPos = currentNode.position + savedMoveDelta;
            targetWorldPos = new Vector3(targetPos.x, 0, targetPos.y);
        }

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

    private void HighlightEdge(Vector2Int startNode, Vector2Int endNode)
    {
        GameObject lineObj = new GameObject("HighlightEdge");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.widthMultiplier = 0.1f;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.alignment = LineAlignment.View;
        
        Vector3 startPos = new Vector3(startNode.x, 0.002f, startNode.y);
        Vector3 endPos = new Vector3(endNode.x, 0.002f, endNode.y);
        
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        
        lr.material.color = new Color32(0x3F, 0x3F, 0x3F, 0xFF); 

        PulseEdgeAnimation anim = lineObj.AddComponent<PulseEdgeAnimation>();
        anim.StartPulsing();
        activeEdgeAnimations.Add(anim);
    }

    private void StopAllEdgeAnimations()
    {
        foreach (var anim in activeEdgeAnimations)
        {
            if (anim != null)
            {
                anim.StopPulsing();
                Destroy(anim.gameObject);
            }
        }
        activeEdgeAnimations.Clear();
    }
}
