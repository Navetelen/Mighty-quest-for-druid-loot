using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileType tileType = TileType.Walkable;

    public bool hasTrap = false;
    public Vector2Int gridPosition;
    public bool isEnemyPath = false;

    public bool isBlocked = false;

    public void SetHighlight(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    public void SetBlocked(bool blocked)
    {
        tileType = blocked ? TileType.HighObstacle : TileType.Walkable;
    }

    public void SetTileType(TileType newType)
    {
        tileType = newType;
        var renderer = GetComponent<Renderer>();

        switch (tileType)
        {
            case TileType.Walkable:
                renderer.material.color = Color.green;
                break;
            case TileType.LowObstacle:
                renderer.material.color = Color.yellow;
                break;
            case TileType.HighObstacle:
                renderer.material.color = Color.red;
                break;
            case TileType.Goal:
                renderer.material.color = Color.cyan;
                break;
        }
    }

    public bool IsWalkable()
    {
        return tileType == TileType.Walkable;
    }
}
