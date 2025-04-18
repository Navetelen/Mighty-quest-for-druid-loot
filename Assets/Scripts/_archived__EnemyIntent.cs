using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnemyIntent : MonoBehaviour
{
    public int spawnTurn = 1;

    [Tooltip("Editorban add meg a grid koordinátákat (x,y)")]
    public List<Vector2Int> pathCoordinates = new List<Vector2Int>();

    [HideInInspector]
    public List<Tile> pathTiles = new List<Tile>();

    public GameObject previewObject;
    private GameObject currentPreviewInstance;
    public bool hasSpawned = false;

    private void OnValidate()
    {
        // Csak ha már létezik GridManager
        if (!Application.isPlaying && GridManagerExists())
        {
            UpdatePathTiles();
        }
    }

    private void Update()
    {


        // Preview ghost ellenfél elhelyezés (editorban is!)
        if (!Application.isPlaying)
        {
            ShowGhostPreview();
        }
    }

    private bool GridManagerExists()
    {
        return FindObjectOfType<GridManager>() != null;
    }

    public void UpdatePathTiles()
    {
        pathTiles.Clear();
        var grid = FindObjectOfType<GridManager>();
        
        if (grid == null)
        {
            Debug.LogWarning("Nincs GridManager a jelenetben!");
            return;
        }

        foreach (var coord in pathCoordinates)
        {
            Tile tile = grid.GetTile(coord);
            if (tile != null)
            {
                pathTiles.Add(tile);
            }
            else
            {
                Debug.LogWarning($"Tile nem található: {coord}");
            }
        }
    }
    void ShowGhostPreview()
    {
        if (previewObject == null || pathTiles.Count == 0)
            return;

        if (currentPreviewInstance == null)
        {
            Debug.Log("Szellem objektum idézése");
            currentPreviewInstance = Instantiate(previewObject);
            currentPreviewInstance.name = "GhostPreview";
        }

        currentPreviewInstance.transform.position = pathTiles[0].transform.position + Vector3.up * 0.5f;
    }

    private void OnDrawGizmos()
    {
        if (pathTiles == null || pathTiles.Count == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < pathTiles.Count - 1; i++)
        {
            Vector3 from = pathTiles[i].transform.position + Vector3.up * 0.2f;
            Vector3 to = pathTiles[i + 1].transform.position + Vector3.up * 0.2f;
            Gizmos.DrawLine(from, to);
            Gizmos.DrawSphere(from, 0.1f);
        }
    }
}
