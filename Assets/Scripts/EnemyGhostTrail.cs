using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGhostTrail : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GameObject arrowHeadPrefab; // ÚJ

    public int stepsAhead = 1;

    private GameObject currentGhost;
    private GameObject arrowInstance; // ÚJ
    private List<Tile> path;
    private DummyEnemy dummy;

    private LineRenderer lineRenderer;
    private float lineHeight = 0.5f; // egységes magasság minden pontnak

    void Awake(){
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.useWorldSpace = true;
    }

    private void Start()
    {
        dummy = GetComponent<DummyEnemy>();
        TurnManager.Instance.OnTurnAdvanced += HandlePhaseChange;
        if (TurnManager.Instance.currentPhase == GamePhase.Planning)
        {
            ShowGhosts();
        }
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnTurnAdvanced -= HandlePhaseChange;
    }

    public void HandlePhaseChange()
    {
        if (TurnManager.Instance.currentPhase == GamePhase.Planning)
        {
            ShowGhosts();
        }
        else
        {
            HideGhost();
        }
    }

    public void ShowGhosts()
    {
        if (dummy == null || dummy.path == null || dummy.path.Count == 0)
            return;

        if (currentGhost != null)
            Destroy(currentGhost);
        if (arrowInstance != null) // Töröljük az előző nyilat, ha van
            Destroy(arrowInstance);

        Tile targetTile = dummy.GetProjectedTargetTile();
        if (targetTile == null)
            return;

        currentGhost = Instantiate(ghostPrefab, targetTile.transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);

        // LineRenderer pontok
        List<Vector3> points = new List<Vector3>();
        Vector3 start = dummy.transform.position;
        start.y = lineHeight;
        points.Add(start);

        int stepIndex = dummy.currentStep;
        int maxSteps = dummy.movementPerTurn;
        int actualSteps = 0;
        bool blockedImmediately = false;

        for (int i = 0; i < maxSteps && stepIndex < dummy.path.Count - 1; i++)
        {
            Tile nextTile = dummy.path[stepIndex + 1];

            if (TileReservationManager.Instance.IsReserved(nextTile) &&
                nextTile.tileType != TileType.Goal)
            {
                if (i == 0) blockedImmediately = true;
                break;
            }

            stepIndex++;
            Vector3 point = nextTile.transform.position;
            point.y = lineHeight;
            points.Add(point);
            actualSteps++;
        }

        // Ha van legalább két pont, lerövidítjük a végét
        if (points.Count >= 2)
        {
            Vector3 last = points[points.Count - 1];
            Vector3 secondLast = points[points.Count - 2];
            Vector3 dir = (last - secondLast).normalized;

            float shortenDistance = 0.5f;
            Vector3 shortenedEnd = last - dir * shortenDistance;
            points[points.Count - 1] = shortenedEnd;

            // Nyíl prefab létrehozása
            if (arrowHeadPrefab != null)
            {
                arrowInstance = Instantiate(arrowHeadPrefab, shortenedEnd, Quaternion.identity, transform);

                arrowInstance.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        // Szín meghatározás
        if (blockedImmediately)
            SetTrailColor(Color.red);
        else if (actualSteps < dummy.movementPerTurn)
            SetTrailColor(Color.yellow);
        else
            SetTrailColor(Color.blue);
    }

    void SetTrailColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    void HideGhost()
    {
        if (currentGhost != null)
            Destroy(currentGhost);
        if (arrowInstance != null)
            Destroy(arrowInstance);

        lineRenderer.positionCount = 0;
    }
}
