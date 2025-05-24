using UnityEngine;
using System.Collections.Generic;

public class LineDebugger : MonoBehaviour
{
    private LogicManager logicManager;
    private Node lastNode;
    private List<GameObject> lineObjects = new List<GameObject>();
    public Material lineMaterial;

    void Start()
    {
        logicManager = FindFirstObjectByType<LogicManager>();
        lastNode = null;
    }

    void Update()
    {
        if (logicManager == null)
            return;

        if (lastNode == null)
        {
            lastNode = logicManager.GetCurrentNode();
            if (lastNode == null)
                return;

            Debug.Log($"[LinesVisualizer] Initialized. Starting at node ({lastNode.position.x}, {lastNode.position.y})");
        }

        Node currentNode = logicManager.GetCurrentNode();
        if (currentNode != lastNode)
        {
            Debug.Log($"[LinesVisualizer] Current node changed to ({currentNode.position.x}, {currentNode.position.y})");
            lastNode = currentNode;

            // Remove old lines
            foreach (var obj in lineObjects)
                Destroy(obj);
            lineObjects.Clear();

            for (int i = 0; i < currentNode.connections.Length; i++)
            {
                if (currentNode.connections[i])
                {
                    Direction dir = (Direction)i;
                    Vector2Int offset = DirectionUtils.DirectionToOffset(dir);
                    Vector2Int neighborPos = currentNode.position + offset;

                    Node neighbor = GetNodeAtPosition(neighborPos);
                    if (neighbor != null)
                    {
                        Vector3 start = new Vector3(currentNode.position.x, 0.2f, currentNode.position.y);
                        Vector3 end = new Vector3(neighbor.position.x, 0.2f, neighbor.position.y);
                        CreateLine(start, end);
                        Debug.Log($"[LinesVisualizer] Drew persistent line from ({start.x}, {start.y}, {start.z}) to ({end.x}, {end.y}, {end.z})");
                    }
                }
            }
        }
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("NodeConnectionLine");
        lineObj.transform.parent = this.transform;
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lineObjects.Add(lineObj);
    }

    private Node GetNodeAtPosition(Vector2Int pos)
    {
        var boardField = typeof(LogicManager).GetField("board", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Node[,] board = boardField.GetValue(logicManager) as Node[,];
        if (board == null) return null;

        int width = board.GetLength(0);
        int height = board.GetLength(1);

        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            return board[pos.x, pos.y];
        return null;
    }
}
