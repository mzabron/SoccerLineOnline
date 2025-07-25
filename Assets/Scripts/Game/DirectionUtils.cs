using UnityEngine;

public static class DirectionUtils
{
    public static Direction? GetDirectionFromDelta(Vector2Int delta)
    {
        if (delta == new Vector2Int(0, 1)) return Direction.N;
        if (delta == new Vector2Int(1, 1)) return Direction.NE;
        if (delta == new Vector2Int(1, 0)) return Direction.E;
        if (delta == new Vector2Int(1, -1)) return Direction.SE;
        if (delta == new Vector2Int(0, -1)) return Direction.S;
        if (delta == new Vector2Int(-1, -1)) return Direction.SW;
        if (delta == new Vector2Int(-1, 0)) return Direction.W;
        if (delta == new Vector2Int(-1, 1)) return Direction.NW;
        return null;
    }

    public static Vector2Int DirectionToOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.N: return new Vector2Int(0, 1);
            case Direction.NE: return new Vector2Int(1, 1);
            case Direction.E: return new Vector2Int(1, 0);
            case Direction.SE: return new Vector2Int(1, -1);
            case Direction.S: return new Vector2Int(0, -1);
            case Direction.SW: return new Vector2Int(-1, -1);
            case Direction.W: return new Vector2Int(-1, 0);
            case Direction.NW: return new Vector2Int(-1, 1);
            default: return Vector2Int.zero;
        }
    }

    public static Direction GetOppositeDirection(Direction dir)
    {
        return (Direction)(((int)dir + 4) % 8);
    }


}
