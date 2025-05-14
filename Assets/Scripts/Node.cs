using UnityEngine;

public class NodeData
{
    public Vector2Int position;
    public bool[] connections = new bool[8]; // N, NE, E, SE, S, SW, W, NW

    public NodeData(int x, int y)
    {
        position = new Vector2Int(x, y);
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

