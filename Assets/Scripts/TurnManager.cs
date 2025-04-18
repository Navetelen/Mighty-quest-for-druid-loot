using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { PlayerBuild, Planning, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public GamePhase currentPhase = GamePhase.PlayerBuild;
    public int currentTurn = 1;

    public event System.Action OnTurnAdvanced;

    

    void Awake()
    {
        Instance = this;
    }

    void Start(){
        Debug.Log($"Turn {currentTurn} | Phase: {currentPhase}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceTurn();
        }
    }

    void AdvanceTurn()
    {
        if (currentPhase == GamePhase.PlayerBuild)
        {
            currentPhase = GamePhase.Planning;
        }
        else if (currentPhase == GamePhase.EnemyTurn) // Building → Planning
        {
            currentTurn++;
            currentPhase = GamePhase.Planning;
        }
        else // Planning → EnemyTurn
        {
            TileReservationManager.Instance.ClearReservations();
            currentPhase = GamePhase.EnemyTurn;
        }

        //Debug.Log($"Turn {currentTurn} | Phase: {currentPhase}");
        OnTurnAdvanced?.Invoke();
    }
}

