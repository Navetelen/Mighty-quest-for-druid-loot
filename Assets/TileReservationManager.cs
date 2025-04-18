using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileReservationManager : MonoBehaviour
{
    public static TileReservationManager Instance;

    public HashSet<Tile> reservedTiles = new HashSet<Tile>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        TurnManager.Instance.OnTurnAdvanced += ClearReservations;
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnAdvanced -= ClearReservations;
    }

    public bool IsReserved(Tile tile)
    {
        if (tile.tileType == TileType.Goal)
            return false; // célmező sosem blokkol

        return reservedTiles.Contains(tile);
    }

    public void Reserve(Tile tile)
    {
        reservedTiles.Add(tile);
    }

    public void ClearReservations()
    {
        reservedTiles.Clear();
    }

    public void Release(Tile tile)
    {
        if (reservedTiles.Contains(tile))
        {
            reservedTiles.Remove(tile);
            Debug.Log("Mező felszabadítva: " + tile.gridPosition);
        }
    }
}
