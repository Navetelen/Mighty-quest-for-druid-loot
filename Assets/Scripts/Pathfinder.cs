using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    private GridManager grid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ha már van egy másik példány
            return;
        }
        Instance = this;
        grid = FindObjectOfType<GridManager>();
    }

    public List<Tile> FindPath(Vector2Int start, Vector2Int end, EnemyBehaviorType behavior)
    {
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();

        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> gScore = new Dictionary<Tile, float>();
        Dictionary<Tile, float> fScore = new Dictionary<Tile, float>();

        Tile startTile = grid.GetTile(start);
        Tile endTile = grid.GetTile(end);

        openSet.Add(startTile);
        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, endTile);

        while (openSet.Count > 0)
        {
            Tile current = GetTileWithLowestFScore(openSet, fScore);

            if (current == endTile)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Tile neighbor in grid.GetNeighbours(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                if (!IsTileWalkable(neighbor, behavior)) continue;

                float tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, endTile);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Tile>(); // nincs elérhető út
    }

    float Heuristic(Tile a, Tile b)
    {
        return Vector2Int.Distance(a.gridPosition, b.gridPosition);
    }

    Tile GetTileWithLowestFScore(List<Tile> openSet, Dictionary<Tile, float> fScore)
    {
        Tile best = openSet[0];
        float bestScore = fScore.ContainsKey(best) ? fScore[best] : Mathf.Infinity;

        foreach (Tile tile in openSet)
        {
            float score = fScore.ContainsKey(tile) ? fScore[tile] : Mathf.Infinity;
            if (score < bestScore)
            {
                best = tile;
                bestScore = score;
            }
        }

        return best;
    }

    List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> totalPath = new List<Tile> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    bool IsTileWalkable(Tile tile, EnemyBehaviorType behavior)
    {
        switch (behavior)
        {
            case EnemyBehaviorType.Flyer:
                return tile.tileType != TileType.HighObstacle;

            case EnemyBehaviorType.Walker:
            case EnemyBehaviorType.Support:
            case EnemyBehaviorType.Berserker:
                return tile.tileType == TileType.Walkable || tile.tileType == TileType.Goal;

            case EnemyBehaviorType.Jumper:
                return true;

            default:
                return false;
        }
    }
}
