using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public List<Vector2Int> spawnPoints;
    public Vector2Int endPoint;

    public int lastSpawnTurn = -999;
    public int spawnCooldown = 3;

    void Start()
    {
        TurnManager.Instance.OnTurnAdvanced += SpawnEnemiesThisTurn;
    }

    void OnDestroy()
    {
        TurnManager.Instance.OnTurnAdvanced -= SpawnEnemiesThisTurn;
    }

    void SpawnEnemiesThisTurn()
    {
        if (TurnManager.Instance.currentTurn - lastSpawnTurn >= spawnCooldown)
        {
            // Egyszerű teszteléshez minden körben minden spawn ponton jön egy enemy
            foreach (var spawn in spawnPoints)
            {
                
                SpawnEnemy(spawn, endPoint);
            }
            lastSpawnTurn = TurnManager.Instance.currentTurn;
        }
    }

    void SpawnEnemy(Vector2Int start, Vector2Int end)
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        Tile tile = grid.GetTile(start);
        if (tile == null || !tile.IsWalkable()) return;

        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = tile.transform.position + Vector3.up * 0.5f;

        DummyEnemy dummy = enemy.GetComponent<DummyEnemy>();
        dummy.startCoords = start;
        dummy.endCoords = end;
    }

}
