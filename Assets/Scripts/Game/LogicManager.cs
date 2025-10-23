using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class LogicManager : MonoBehaviour
{
    private int width = 9;
    private int height = 11;
    public GameObject nodePrefab;
    public GameObject topGoalModel;
    public GameObject bottomGoalModel;
    public GameObject soccer;
    public GameObject field;
    private Node[,] board;
    private Node currentNode;
    public Material lineMaterial;
    private int currentPlayer = 1; // 1 or 2
    private bool isFirstMove = true;
    public bool isGameOver = false;
    private Vector2 swipeStart;
    private bool isSwiping = false;
    private bool swipeDetected = false;
    private UIManager uiManager;
    private bool gameOverScreenShown = false;
    public bool is1TimerRunning = false;
    public bool is2TimerRunning = false;
    public bool isPlayer1TimeLow = false;
    public bool isPlayer2TimeLow = false;

    private int winnerPlayer = 0;
    private string winnerNickname = "";
    private int winnerRating = 0;

    protected float player1Time = 60f;
    protected float player2Time = 60f;
    public TMPro.TMP_Text player1TimerText;
    public TMPro.TMP_Text player2TimerText;

    protected string player1Nickname = "Player1";
    protected string player2Nickname = "Player2";
    private int player1Rating = 2000;
    private int player2Rating = 2000;

    public TMPro.TMP_Text player1NicknameText;
    public TMPro.TMP_Text player2NicknameText;
    public TMPro.TMP_Text player1RatingText;
    public TMPro.TMP_Text player2RatingText;

    protected bool isTutorialMode = false;
    protected bool canMove = true;

    protected virtual void Start()
    {
        GenerateBoard(width, height);
        currentNode = board[(width - 1) / 2, (height - 1) / 2];
        FitPerspectiveCameraToField();
        UpdatePlayerInfoUI();
        uiManager = FindFirstObjectByType<UIManager>();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game by reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    protected virtual void Update()
    {
        if (isGameOver)
        {
            if (!gameOverScreenShown && uiManager != null)
            {
                uiManager.ShowGameOverScreen(winnerNickname, winnerRating);
                gameOverScreenShown = true;
            }
            return;
        }

        if (soccer != null)
        {
            Soccer soccerScript = soccer.GetComponent<Soccer>();
            if (soccerScript != null && soccerScript.movingToGoal)
                return;
        }

        if (Touchscreen.current != null && !UIManager.IsSettingsOpen  && canMove)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                if (!IsScreenPositionOverField(touch.position.ReadValue()))
                    return;
                swipeStart = touch.position.ReadValue();
                isSwiping = true;
                swipeDetected = false;
            }
            else if (isSwiping && touch.press.isPressed)
            {
                Vector2 swipeDelta = touch.position.ReadValue() - swipeStart;
                if (!swipeDetected && swipeDelta.magnitude > 100f && UIManager.IsSwipeMoveEnabled)
                {
                    swipeDetected = true;
                    TrySwipeMove(swipeDelta);
                }
            }
            else if (isSwiping && touch.press.wasReleasedThisFrame)
            {
                Vector2 swipeDelta = touch.position.ReadValue() - swipeStart;
                isSwiping = false;
                if (!swipeDetected && swipeDelta.magnitude < 100f)
                {
                    Vector3 tapWorldPos = GetWorldPosition(touch.position.ReadValue());
                    HandleTap(tapWorldPos, touch.position.ReadValue());
                }
            }
        }

#if UNITY_EDITOR

        if (Mouse.current != null && !UIManager.IsSettingsOpen)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                swipeStart = Mouse.current.position.ReadValue();
                isSwiping = true;
                swipeDetected = false;
            }
            else if (isSwiping && Mouse.current.leftButton.isPressed)
            {
                Vector2 swipeDelta = Mouse.current.position.ReadValue() - swipeStart;
                if (!swipeDetected && swipeDelta.magnitude > 50f && UIManager.IsSwipeMoveEnabled)
                {
                    swipeDetected = true;
                    TrySwipeMove(swipeDelta);
                }
            }
            else if (isSwiping && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 swipeDelta = Mouse.current.position.ReadValue() - swipeStart;
                isSwiping = false;
                if (!swipeDetected && swipeDelta.magnitude < 50f)
                {
                    Vector3 tapWorldPos = GetWorldPosition(Mouse.current.position.ReadValue());
                    HandleTap(tapWorldPos, Mouse.current.position.ReadValue());
                }
            }
        }
#endif

        //---------------------- Game timer logic --------------------------

        if (!isTutorialMode)
        {
            if (currentPlayer == 1)
        {
            is2TimerRunning = false;
            is1TimerRunning = true;
            player1Time -= Time.deltaTime;
            if (player1Time <= 0f)
            {
                player1Time = 0f;
                SetWinner(2); // Player 2 wins by timeout
                isGameOver = true;
                Debug.Log("Player 2 wins by timeout!");
            }
        }
        else
        {
            is1TimerRunning = false;
            is2TimerRunning = true;
            player2Time -= Time.deltaTime;
            if (player2Time <= 0f)
            {
                player2Time = 0f;
                SetWinner(1); // Player 1 wins by timeout
                isGameOver = true;
                Debug.Log("Player 1 wins by timeout!");
            }
        }

        isPlayer1TimeLow = player1Time <= 20f;
        isPlayer2TimeLow = player2Time <= 20f;

        UpdateTimerUI();
        }
    }

    private void SetWinner(int playerNumber)
    {
        winnerPlayer = playerNumber;
        if (playerNumber == 1)
        {
            winnerNickname = player1Nickname;
            winnerRating = player1Rating;
        }
        else
        {
            winnerNickname = player2Nickname;
            winnerRating = player2Rating;
        }
    }

    private bool IsScreenPositionOverField(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == field)
                return true;
        }
        return false;
    }

    private void HandleTap(Vector3 tap, Vector2 screenPosition)
    {
        int x = currentNode.position.x;
        int z = currentNode.position.y;

        bool isBottomGoalAdj = (z == 0) && (x >= 3 && x <= 5);
        bool isTopGoalAdj = (z == 10) && (x >= 3 && x <= 5);

        if (isBottomGoalAdj || isTopGoalAdj)
        {
            GameObject goalModel = isTopGoalAdj ? topGoalModel : bottomGoalModel;

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject == goalModel)
                {
                    float goalX = Mathf.Clamp(tap.x, 3f, 5f);
                    float goalZ = isTopGoalAdj ? 10.7f : -0.7f;
                    Vector3 from = new Vector3(currentNode.position.x, 0.001f, currentNode.position.y);
                    Vector3 to = new Vector3(goalX, 0.001f, goalZ);

                    DrawLine(from, to, true);

                    if (soccer != null)
                    {
                        Soccer soccerScript = soccer.GetComponent<Soccer>();
                        if (soccerScript != null)
                        {
                            soccerScript.MoveToGoal(to);
                        }
                    }

                    // Determine winner based on which goal was scored
                    int winner = DetermineGoalWinner(isTopGoalAdj);
                    SetWinner(winner);
                    return;
                }
            }
        }

        Node closestNode = null;
        float minDist = float.MaxValue;

        for (int x_ = 0; x_ < width; x_++)
        {
            for (int z_ = 0; z_ < height; z_++)
            {
                Vector3 nodePos = new Vector3(x_, 0, z_);
                float dist = Vector3.Distance(tap, nodePos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestNode = board[x_, z_];
                }
            }
        }

        if (closestNode != null && minDist <= 0.5f)
        {
            SelectNode(new Vector3(closestNode.position.x, 0, closestNode.position.y));
        }
    }

    // Determines the winner based on which goal was scored
    // Bottom goal (z = 0) is Player 1's goal
    // Top goal (z = 10) is Player 2's goal
    private int DetermineGoalWinner(bool isTopGoal)
    {
        if (isTopGoal)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (boardPlane.Raycast(ray, out enter))
            return ray.GetPoint(enter);
        return Vector3.zero;
    }

    public void SelectNode(Vector3 worldPosition)
    {
        if (isGameOver)
            return;

        int x = Mathf.RoundToInt(worldPosition.x);
        int z = Mathf.RoundToInt(worldPosition.z);

        if (x < 0 || x >= width || z < 0 || z >= height)
            return;

        Node selectedNode = board[x, z];

        List<Node> neighbors = CheckForNeighbors();
        if (!neighbors.Contains(selectedNode))
            return;

        Vector2Int delta = selectedNode.position - currentNode.position;
        Direction? dir = DirectionUtils.GetDirectionFromDelta(delta);
        if (dir == null)
            return;

        if (currentNode.IsConnected(dir.Value))
            return;

        Debug.Log($"Selected node at ({selectedNode.position.x}, {selectedNode.position.y})");

        // ------------------------ Drawing lines ------------------------
        Vector3 from = new Vector3(currentNode.position.x, 0.001f, currentNode.position.y);
        Vector3 to = new Vector3(selectedNode.position.x, 0.001f, selectedNode.position.y);

        DrawLine(from, to, false);

        currentNode.ConnectTo(dir.Value);
        Direction oppositeDir = DirectionUtils.GetOppositeDirection(dir.Value);
        selectedNode.ConnectTo(oppositeDir);
        currentNode = selectedNode;

        if (CheckForBlock())
        {
            int winner = (currentPlayer == 1) ? 2 : 1; // The other player wins when current player is blocked
            SetWinner(winner);
            isGameOver = true;

            Soccer soccerScript = this.soccer.GetComponent<Soccer>();
            if (soccerScript != null)
                soccerScript.MoveToGoal(to);
            return;
        }

        // -------------------- Turns Logic -----------------------

        int connectionCount = 0;
        foreach (bool c in selectedNode.connections)
            if (c) connectionCount++;

        if (isFirstMove)
        {
            isFirstMove = false;
            SwitchTurn();
            return;
        }

        if (connectionCount == 1)
        {
            SwitchTurn();
        }
    }

    private void TrySwipeMove(Vector2 swipeDelta)
    {
        if (swipeDelta.magnitude < 50f)
            return;

        float angle = Mathf.Atan2(swipeDelta.y, swipeDelta.x) * Mathf.Rad2Deg;
        Direction? dir = null;

        if (angle >= -22.5f && angle < 22.5f)
            dir = Direction.E;
        else if (angle >= 22.5f && angle < 67.5f)
            dir = Direction.NE;
        else if (angle >= 67.5f && angle < 112.5f)
            dir = Direction.N;
        else if (angle >= 112.5f && angle < 157.5f)
            dir = Direction.NW;
        else if (angle >= 157.5f || angle < -157.5f)
            dir = Direction.W;
        else if (angle >= -157.5f && angle < -112.5f)
            dir = Direction.SW;
        else if (angle >= -112.5f && angle < -67.5f)
            dir = Direction.S;
        else if (angle >= -67.5f && angle < -22.5f)
            dir = Direction.SE;

        int x = currentNode.position.x;
        int z = currentNode.position.y;
        bool isBottomGoalAdj = (z == 0) && (x >= 3 && x <= 5);
        bool isTopGoalAdj = (z == 10) && (x >= 3 && x <= 5);

        if (isTopGoalAdj && dir != null)
        {
            float goalY = 0.001f, goalZ = 10.7f;
            if (x == 3)
            {
                if (dir == Direction.NW) return;
                if (dir == Direction.N)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(3, goalY, goalZ), true);
                else if (dir == Direction.NE)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), true);
                else
                    goto normalSwipe;
                return;
            }
            else if (x == 5)
            {
                if (dir == Direction.NE) return;
                if (dir == Direction.N)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(5, goalY, goalZ), true);
                else if (dir == Direction.NW)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), true);
                else
                    goto normalSwipe;
                return;
            }
            else if (x == 4)
            {
                if (dir == Direction.N)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), true);
                else if (dir == Direction.NW)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(3, goalY, goalZ), true);
                else if (dir == Direction.NE)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(5, goalY, goalZ), true);
                else
                    goto normalSwipe;
                return;
            }
        }
        if (isBottomGoalAdj && dir != null)
        {
            float goalY = 0.001f, goalZ = -0.7f;
            if (x == 3)
            {
                if (dir == Direction.SW) return;
                if (dir == Direction.S)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(3, goalY, goalZ), false);
                else if (dir == Direction.SE)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), false);
                else
                    goto normalSwipe;
                return;
            }
            else if (x == 5)
            {
                if (dir == Direction.SE) return;
                if (dir == Direction.S)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(5, goalY, goalZ), false);
                else if (dir == Direction.SW)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), false);
                else
                    goto normalSwipe;
                return;
            }
            else if (x == 4)
            {
                if (dir == Direction.S)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(4, goalY, goalZ), false);
                else if (dir == Direction.SW)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(3, goalY, goalZ), false);
                else if (dir == Direction.SE)
                    DrawGoalLineAndMoveSoccer(new Vector3(x, goalY, z), new Vector3(5, goalY, goalZ), false);
                else
                    goto normalSwipe;
                return;
            }
        }

    normalSwipe:
        if (dir != null)
        {
            Vector2Int offset = DirectionUtils.DirectionToOffset(dir.Value);
            Vector2Int newPos = currentNode.position + offset;

            if (newPos.x >= 0 && newPos.x < width && newPos.y >= 0 && newPos.y < height)
            {
                Node neighbor = board[newPos.x, newPos.y];
                List<Node> neighbors = CheckForNeighbors();
                if (neighbor != null && neighbors.Contains(neighbor))
                {
                    SelectNode(new Vector3(neighbor.position.x, 0, neighbor.position.y));
                }
            }
        }
    }

    private void DrawGoalLineAndMoveSoccer(Vector3 from, Vector3 to, bool isTopGoal)
    {
        DrawLine(from, to, true);

        if (soccer != null)
        {
            Soccer soccer = this.soccer.GetComponent<Soccer>();
            if (soccer != null)
                soccer.MoveToGoal(to);
                
            // Determine winner based on which goal was scored
            int winner = DetermineGoalWinner(isTopGoal);
            SetWinner(winner);
        }
    }

    private void DrawLine(Vector3 from, Vector3 to, bool isGoalLine)
    {
        string lineName = isGoalLine ? "GoalLine" : "Line";
        GameObject lineObj = new GameObject(lineName);
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.widthMultiplier = 0.1f;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.alignment = LineAlignment.View;

        Color playerColor = (currentPlayer == 1) ? new Color32(0x6e, 0xc3, 0xff, 0xff) : new Color32(0xFF, 0x77, 0x79, 0xFF);
        Color baseColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF);

        if (UIManager.IsAnimationsEnabled)
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, from);
            lr.material.color = playerColor;

            StartCoroutine(AnimateLine(lr, from, to, 0.25f));
            StartCoroutine(FadeOut(lr, playerColor, baseColor, 2f));
        }
        else
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
            lr.material.color = baseColor;
        }
    }

    private System.Collections.IEnumerator FadeOut(LineRenderer lr, Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Color lerped = Color.Lerp(fromColor, toColor, t);
            lr.material.color = lerped;
            elapsed += Time.deltaTime;
            yield return null;
        }
        lr.material.color = toColor;
    }

    private System.Collections.IEnumerator AnimateLine(LineRenderer lr, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            lr.SetPosition(1, Vector3.Lerp(from, to, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        lr.SetPosition(1, to);
    }

    List<Node> CheckForNeighbors()
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int pos = currentNode.position;

        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2Int offset = DirectionUtils.DirectionToOffset(dir);
            int x = pos.x + offset.x;
            int y = pos.y + offset.y;

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                Node neighbor = board[x, y];
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }
        }
        return neighbors;

    }

    public bool CheckForBlock()
    {
        Vector2Int pos = currentNode.position;
        int x = pos.x;
        int y = pos.y;

        int connectionCount = 0;
        foreach (bool c in currentNode.connections)
            if (c) connectionCount++;

        bool isCenter = (x >= 1 && x <= 7) && (y >= 1 && y <= 9);
        bool isLeftBorder = (x == 0) && (y >= 1 && y <= 9);
        bool isRightBorder = (x == 8) && (y >= 1 && y <= 9);
        bool isBottomBorder = (y == 0) && (x >= 1 && x <= 7);
        bool isTopBorder = (y == 10) && (x >= 1 && x <= 7);
        bool isGoalAdjacent = ((x >= 3 && x <= 5) && (y == 0 || y == 10));
        bool isCorner = (x == 0 && y == 0) || (x == 0 && y == 10) ||
                        (x == 8 && y == 0) || (x == 8 && y == 10);

        if (isCenter)
        {
            if (connectionCount == 8)
                return true;
        }

        if ((isLeftBorder || isRightBorder || isBottomBorder || isTopBorder) && !isGoalAdjacent)
        {
            if (connectionCount == 5)
                return true;
        }

        if (isCorner)
        {
            if (connectionCount == 3)
                return true;
        }

        return false;
    }

    private void SwitchTurn()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        Debug.Log($"Now it's Player {currentPlayer}'s turn!");
    }

    void GenerateBoard(int width, int height)
    {
        board = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node node = new Node(x, z);
                board[x, z] = node;

                if (nodePrefab != null)
                {
                    Vector3 position = new Vector3(x, 0.01f, z);
                    Instantiate(nodePrefab, position, Quaternion.identity);
                }
            }
        }

        //Connecting the border nodes to make field border moves impossible
        // except for the goal gaps at (3,10)-(4,10), (4,10)-(5,10), (3,0)-(4,0), (4,0)-(5,0)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node node = board[x, z];

                bool isLeft = x == 0;
                bool isRight = x == width - 1;
                bool isTop = z == height - 1;
                bool isBottom = z == 0;

                // Corners
                if ((isLeft && isBottom) || (isLeft && isTop) || (isRight && isBottom) || (isRight && isTop))
                {
                    if (!isTop)
                    {
                        node.ConnectTo(Direction.N);
                        board[x, z + 1].ConnectTo(Direction.S);
                    }
                    if (!isBottom)
                    {
                        node.ConnectTo(Direction.S);
                        board[x, z - 1].ConnectTo(Direction.N);
                    }
                    if (!isLeft)
                    {
                        node.ConnectTo(Direction.W);
                        board[x - 1, z].ConnectTo(Direction.E);
                    }
                    if (!isRight)
                    {
                        node.ConnectTo(Direction.E);
                        board[x + 1, z].ConnectTo(Direction.W);
                    }
                }
                // Left/Right border nodes (not corners)
                else if (isLeft || isRight)
                {
                    if (z + 1 < height)
                    {
                        node.ConnectTo(Direction.N);
                        board[x, z + 1].ConnectTo(Direction.S);
                    }
                    if (z - 1 >= 0)
                    {
                        node.ConnectTo(Direction.S);
                        board[x, z - 1].ConnectTo(Direction.N);
                    }
                }
                // Top/Bottom border nodes (not corners)
                else if (isTop || isBottom)
                {
                    if (x - 1 >= 0 && !IsGoalGap(x, x - 1, z))
                    {
                        node.ConnectTo(Direction.W);
                        board[x - 1, z].ConnectTo(Direction.E);
                    }
                    if (x + 1 < width && !IsGoalGap(x, x + 1, z))
                    {
                        node.ConnectTo(Direction.E);
                        board[x + 1, z].ConnectTo(Direction.W);
                    }
                }

            }
        }
    }

    private bool IsGoalGap(int x1, int x2, int z)
    {
        if ((z == 10) && (
            (x1 == 3 && x2 == 4) || (x1 == 4 && x2 == 3) ||
            (x1 == 4 && x2 == 5) || (x1 == 5 && x2 == 4)))
            return true;
        if ((z == 0) && (
            (x1 == 3 && x2 == 4) || (x1 == 4 && x2 == 3) ||
            (x1 == 4 && x2 == 5) || (x1 == 5 && x2 == 4)))
            return true;
        return false;
    }

    public Node GetCurrentNode()
    {
        return currentNode;
    }

    private void UpdateTimerUI()
    {
        if (player1TimerText != null)
            player1TimerText.text = FormatTime(player1Time);
        if (player2TimerText != null)
            player2TimerText.text = FormatTime(player2Time);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdatePlayerInfoUI()
    {
        if (player1NicknameText != null)
            player1NicknameText.text = player1Nickname;
        if (player2NicknameText != null)
            player2NicknameText.text = player2Nickname;
        if (player1RatingText != null)
            player1RatingText.text = player1Rating.ToString();
        if (player2RatingText != null)
            player2RatingText.text = player2Rating.ToString();
    }

    private void FitPerspectiveCameraToField()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float fieldWidth = width;
        float fieldHeight = height;

        Vector3 center = new Vector3((width - 1) / 2f, 0, (height - 1) / 2f);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        float aspect = (float)Screen.width / Screen.height;
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;

        float halfFieldWidth = fieldWidth / 2f;
        float halfFieldHeight = fieldHeight / 2f;

        float distanceForHeight = halfFieldHeight / Mathf.Tan(fovRad / 2f);
        float distanceForWidth = halfFieldWidth / (Mathf.Tan(fovRad / 2f) * aspect);

        float verticalThreshold = 4.0f;
        float heightWhenFittingWidth = Mathf.Tan(fovRad / 2f) * distanceForWidth;
        
        if (heightWhenFittingWidth < (halfFieldHeight + verticalThreshold))
        {
            float requiredDistance = distanceForHeight + verticalThreshold;
            cam.transform.position = center + new Vector3(0, requiredDistance, 0);
        }
        else
        {
            float requiredDistance = distanceForWidth;
            cam.transform.position = center + new Vector3(0, requiredDistance, 0);
        }

        cam.rect = new Rect(0f, -0.2f, 1f, 1.2f);
    }
}

