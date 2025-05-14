using UnityEngine;

public class LogicManager : MonoBehaviour
{
    private int width = 9;
    private int height = 11;
    public GameObject nodePrefab;

    private NodeData[,] board;

    void Start()
    {
        GenerateBoard(width, height);
    }

    void GenerateBoard(int width, int height)
    {
        board = new NodeData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                NodeData node = new NodeData(x, y);
                board[x, y] = node;

                if (nodePrefab != null)
                {
                    Vector3 position = new Vector3(x, y, 0);
                    Instantiate(nodePrefab, position, Quaternion.identity);
                }
            }
        }
    }
}

