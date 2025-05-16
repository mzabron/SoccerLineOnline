using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
    private int width = 9;
    private int height = 11;
    public GameObject nodePrefab;
    private Node[,] board; // 2D array of nodes, I'm using just x and y coordinates to
                           // describe position of each node. However, in reality it's x
                           // and z, while y is constant(0).
    private Node currentNode;
    private bool isWhiteTurn = true;

    void Start()
    {
        GenerateBoard(width, height);
        currentNode = board[(width - 1) / 2, (height - 1) / 2]; 
    }

    void Update()
    {
        Vector3? tapWorldPos = null;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapWorldPos = GetWorldPosition(Input.GetTouch(0).position);
        }
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            tapWorldPos = GetWorldPosition(Input.mousePosition);
        }
#endif

        if (tapWorldPos.HasValue)
        {
            Node closestNode = null;
            float minDist = float.MaxValue;
            Vector3 tap = tapWorldPos.Value;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 nodePos = new Vector3(x, 0, z);
                    float dist = Vector3.Distance(tap, nodePos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestNode = board[x, z];
                    }
                }
            }

            if (closestNode != null && minDist <= 0.5f)
            {
                SelectNode(new Vector3(closestNode.position.x, 0, closestNode.position.y));
            }
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
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.z);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        Node selectedNode = board[x, y];

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


        currentNode.ConnectTo(dir.Value);
        Debug.Log($"Connected from ({currentNode.position.x}, {currentNode.position.y}) to direction {dir.Value}");


        Direction oppositeDir = DirectionUtils.GetOppositeDirection(dir.Value);
        selectedNode.ConnectTo(oppositeDir);
        Debug.Log($"Connected from ({selectedNode.position.x}, {selectedNode.position.y}) to direction {oppositeDir}");



        currentNode = selectedNode;
    }
    void Move()
    {
        CheckForNeighbors();

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
                    Vector3 position = new Vector3(x, 0, z);
                    Instantiate(nodePrefab, position, Quaternion.identity);
                }
            }
        }
    }
}

