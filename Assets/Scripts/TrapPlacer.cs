using UnityEngine;

public class TrapPlacer : MonoBehaviour
{
    public GameObject[] trapPrefabs; // 0 = FireTrap, 1 = IceTrap

    private int selectedTrapIndex = 0;
    private GameObject currentPreview;
    private bool isPlacing = false;

    void Update()
    {
        if (TurnManager.Instance.currentPhase != GamePhase.Planning)
        {
            ClearPreview();
            return;
        }

        HandleInput();

        if (isPlacing)
        {
            UpdatePreviewPosition();
        }
        else
        {
            ClearPreview();
        }
    }

    void HandleInput()
    {
        // Építés mód ki/be kapcsolása
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isPlacing = !isPlacing;
            if (!isPlacing)
                ClearPreview();
        }

        // Trap típus választás
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTrapIndex = 0;
            RefreshPreview();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTrapIndex = 1;
            RefreshPreview();
        }
    }

    void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int tileLayerMask = 1 << LayerMask.NameToLayer("Tile");
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayerMask))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null && !tile.hasTrap)
            {
                if (currentPreview == null)
                {
                    currentPreview = Instantiate(trapPrefabs[selectedTrapIndex]);
                    currentPreview.transform.localScale = Vector3.one * 0.4f;
                }

                currentPreview.transform.position = tile.transform.position + Vector3.up * 0.1f;

                if (Input.GetMouseButtonDown(0))
                {
                    PlaceTrap(tile);
                }
            }
        }
    }

    void RefreshPreview()
    {
        // Ha van már preview, cseréljük le
        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = Instantiate(trapPrefabs[selectedTrapIndex]);
        currentPreview.transform.localScale = Vector3.one * 0.4f;
    }

    void PlaceTrap(Tile tile)
    {
        GameObject trap = Instantiate(trapPrefabs[selectedTrapIndex], tile.transform.position + Vector3.up * 0.1f, Quaternion.identity);
        trap.transform.localScale = Vector3.one * 0.7f;
        tile.hasTrap = true;

        // Visszaállítjuk a preview-t, hogy lehessen tovább építeni
        RefreshPreview();
    }

    void ClearPreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = null;
    }
}
