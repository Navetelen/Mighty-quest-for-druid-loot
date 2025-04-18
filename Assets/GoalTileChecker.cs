using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTileChecker : MonoBehaviour
{
    private GridManager grid;

    void Start()
    {
        grid = FindObjectOfType<GridManager>();
        TurnManager.Instance.OnTurnAdvanced += CheckGoalTile;
    }

    void OnDestroy()
    {
        TurnManager.Instance.OnTurnAdvanced -= CheckGoalTile;
    }

    void CheckGoalTile()
    {
        foreach (var tile in grid.tiles)
        {
            if (tile.tileType == TileType.Goal)
            {
                Collider[] hits = Physics.OverlapSphere(tile.transform.position + Vector3.up * 0.5f, 0.1f);
                int removedEnemies = 0;

                foreach (var hit in hits)
                {
                    DummyEnemy enemy = hit.GetComponent<DummyEnemy>();
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                        removedEnemies++;
                    }
                }

                if (removedEnemies > 0)
                {
                    EnemyManager.Instance.RegisterMultiple(removedEnemies);
                    Debug.Log($"{removedEnemies} ellenfél beért a célba!");
                }
            }
        }
    }
}
