using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public GameObject tilePrefab;
    public GameObject portalPrefab;

    public Tile[,] tiles;

    

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;
    [Range(0, 0.5f)] public float obstacleChance = 0.1f;

    void Awake()
    {
        GenerateGrid();
    }

    IEnumerator VerifyPathExists(Vector2Int start, Vector2Int goal)
    {
        yield return null; // várunk 1 frame-et

        if (!HasPathToGoal(start, goal))
        {
            Debug.LogWarning("⚠️ Nincs elérhető útvonal! Újratervezés...");

            // Egyszerű megoldás: újrageneráljuk a pályát
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            GenerateGrid();
        }
    }

    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            int openTilesInColumn = 0;

            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tileObj.name = $"Tile_{x}_{y}";

                Tile tile = tileObj.GetComponent<Tile>();
                if (tile == null)
                    tile = tileObj.AddComponent<Tile>();
                tile.gridPosition = new Vector2Int(x, y);
                tile.tileType = TileType.Walkable;
                tiles[x, y] = tile;

                if (Random.value < obstacleChance && y > 0 && y < height - 1)
                {
                    if (openTilesInColumn < height - 1)
                    {
                        GameObject chosenPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                        GameObject obstacleInstance = Instantiate(chosenPrefab, pos + Vector3.up * 0.5f, Quaternion.identity, tileObj.transform);

                        if (obstacleInstance.CompareTag("LowObstacle"))
                        {
                            tile.SetTileType(TileType.LowObstacle);
                        }
                        else if (obstacleInstance.CompareTag("HighObstacle"))
                        {
                            tile.SetTileType(TileType.HighObstacle);
                        }
                        else
                        {
                            tile.SetTileType(TileType.HighObstacle); // fallback, biztonság kedvéért
                        }
                    }
                    else
                    {
                        openTilesInColumn++;
                        tile.SetTileType(TileType.Walkable); // biztosítsuk, hogy kap színt
                    }
                }
                else
                {
                    openTilesInColumn++;
                    if(x == 4 && y == 11)
                    {
                        Debug.Log(tile);
                        tile.SetTileType(TileType.Goal);
                        portalPrefab.transform.localScale *= 0.6f;
                        Instantiate(portalPrefab,tile.transform.position + new Vector3(0,0.5f,0),Quaternion.identity);
                    }
                    else{
                        
                        tile.SetTileType(TileType.Walkable);
                    }
                    
                }


            }
        }
        Vector2Int start = new Vector2Int(width / 2, 0); // vagy ahonnan általában indul az enemy
        Vector2Int goal = new Vector2Int(4, height - 1); // vagy ahol a goal mező van

        StartCoroutine(VerifyPathExists(start, goal));
    }

    bool HasPathToGoal(Vector2Int start, Vector2Int goal)
    {
        var path = Pathfinder.Instance.FindPath(start, goal, EnemyBehaviorType.Walker);
        return path != null && path.Count > 1;
    }


    public List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> neighbours = new List<Tile>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = tile.gridPosition + dir;
            Tile neighbour = GetTile(checkPos);
            if (neighbour != null)
            {
                neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }


    public Tile GetTile(Vector2Int coords)
    {
        if (tiles == null)
        {
            Debug.LogWarning("GridManager: tiles tömb még nem jött létre!");
            return null;
        }

        if (coords.x < 0 || coords.x >= width || coords.y < 0 || coords.y >= height)
        {
            //Debug.LogWarning($"GridManager: érvénytelen koordináta ({coords})");
            return null;
        }

        return tiles[coords.x, coords.y];
    }
}
