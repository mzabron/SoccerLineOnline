using UnityEngine;

public class Node
{
    public Vector2Int position;
    public bool[] connections = new bool[8]; // N, NE, E, SE, S, SW, W, NW

    public Node(int x, int z)
    {
        position = new Vector2Int(x, z);
    }

    public void ConnectTo(Direction dir)
    {
        connections[(int)dir] = true;
    }

    public bool IsConnected(Direction dir)
    {
        return connections[(int)dir];
    }
}

