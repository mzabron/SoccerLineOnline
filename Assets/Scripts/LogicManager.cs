using UnityEngine;

public class LogicManager : MonoBehaviour
{
    private int width = 9;
    private int height = 11;
    public GameObject nodePrefab;

    private Node[,] board;

    void Start()
    {
        GenerateBoard(width, height);
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

