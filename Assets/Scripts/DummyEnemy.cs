using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPhase
{
    Idle,
    Moving,
    UsingAbility,
    CheckingTraps,
    BeingAttacked,
    Done
}

public class DummyEnemy : MonoBehaviour
{
    public List<Tile> path;
    public int currentStep = 0;

    [Header("Movement Settings")]
    public int baseMovementPerTurn;
    public int movementPerTurn;
    public float moveDelay = 0.3f;

    private EnemyPhase currentPhase = EnemyPhase.Idle;
    private bool isProcessing = false;

    [Header("Combat Stats")]
    public int health = 100;

    [Header("VFX")]
    public GameObject deathVFX;

    [Header("Status Effects")]
    public bool isFrozen = false;
    public int frozenTurnsRemaining = 0;

    public bool isSlowed = false;
    public int slowedTurnsRemaining = 0;

    [Header("Status Effect VFX")]
    public GameObject frozen;
    private GameObject activeFrozenVFX;
    public GameObject burning;
    public GameObject bleeding;
    public GameObject electrified;
    public GameObject poisoned;

    [Header("Pathfinding")]
    public EnemyBehaviorType behaviorType = EnemyBehaviorType.Walker;
    public Vector2Int startCoords;
    public Vector2Int endCoords;




    void Awake()
    {
        //baseMovementPerTurn = Random.Range(1, 4);
        movementPerTurn = baseMovementPerTurn;
    }

    private void Start()
    {
        TurnManager.Instance.OnTurnAdvanced += OnTurnPhaseChanged;

        path = Pathfinder.Instance.FindPath(startCoords, endCoords, behaviorType);
        if (TryGetComponent<EnemyGhostTrail>(out var trail))
        {
            Debug.Log("első ghost mutatás lenne ITT");
            Debug.Log(trail);
            trail.ShowGhosts(); // azonnal megjelenik ha planning phase van
        }
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnTurnAdvanced -= OnTurnPhaseChanged;
    }

    void OnTurnPhaseChanged()
    {
        if (TurnManager.Instance.currentPhase == GamePhase.EnemyTurn)
        {
            UpdateStatusEffects();
            currentPhase = EnemyPhase.Moving;
            isProcessing = true;
            StartCoroutine(HandleEnemyTurn());
        }
    }

    public void CalculatePath()
    {
        path = Pathfinder.Instance.FindPath(startCoords, endCoords, behaviorType);
        currentStep = 0;
    }


    void UpdateStatusEffects()
    {
        if (frozenTurnsRemaining > 0)
        {
            if (!isFrozen && frozen != null)
            {
                Debug.Log("Fagyás effekt létrejön!");
                activeFrozenVFX = Instantiate(frozen, transform.position, Quaternion.identity, transform);
            }
            isFrozen = true;
            frozenTurnsRemaining--;
        }
        else
        {
            isFrozen = false;

            // Jégfal eltávolítása, ha már nincs fagyás
            if (activeFrozenVFX != null)
            {
                Destroy(activeFrozenVFX);
                activeFrozenVFX = null;
            }
        }

        if (slowedTurnsRemaining > 0)
        {
            isSlowed = true;
            slowedTurnsRemaining--;
        }
        else
        {
            isSlowed = false;
        }

        if (isFrozen)
        {
            movementPerTurn = 0;
        }
        else
        {
            movementPerTurn = baseMovementPerTurn;
            if (isSlowed)
                movementPerTurn = Mathf.Max(0, movementPerTurn - 1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isFrozen)
        {
            amount *= 2;
            isFrozen = false;
            frozenTurnsRemaining = 0;
            health -= amount;
            slowedTurnsRemaining--;
            Debug.Log($"{gameObject.name} megfagyva volt – dupla sebzést kap: {amount}");
        }
        else
        {
            health -= amount;
            Debug.Log($"{gameObject.name} sebzést kap: {amount}");
        }

        // Később ide jöhet életerő kezelés
        if(health <= 0){
            Die();
        }
    }

    IEnumerator HandleEnemyTurn()
    {
        while (isProcessing)
        {
            switch (currentPhase)
            {
                case EnemyPhase.Moving:
                    yield return StartCoroutine(StepAlongPath());
                    currentPhase = EnemyPhase.UsingAbility;
                    Debug.Log("Current phase: " + currentPhase);
                    Debug.Log("Az ellenfél mozog");
                    break;

                case EnemyPhase.UsingAbility:
                    // Ide később jönnek az aktív képességek
                    yield return new WaitForSeconds(0.3f);
                    currentPhase = EnemyPhase.CheckingTraps;
                    Debug.Log("Current phase: " + currentPhase);
                    Debug.Log("Az ellenfél csapdákat érzékel");
                    
                    break;

                case EnemyPhase.CheckingTraps:
                    CheckForTrap();
                    yield return new WaitForSeconds(0.2f);
                    currentPhase = EnemyPhase.BeingAttacked;
                    Debug.Log("Current phase: " + currentPhase);
                    Debug.Log("Az ellenfél támadás alatt");
                    break;

                case EnemyPhase.BeingAttacked:
                    // Ide jön majd az egységek támadása
                    yield return new WaitForSeconds(0.2f);
                    currentPhase = EnemyPhase.Done;
                    Debug.Log("Current phase: " + currentPhase);
                    Debug.Log("Az ellenfél végzett");
                    break;

                case EnemyPhase.Done:
                    isProcessing = false;
                    break;
            }

            yield return null;
        }
    }

    IEnumerator StepAlongPath()
    {
        int stepsTaken = 0;
        Tile currentTile = path[currentStep];
        Tile previousTile = currentStep > 0 ? path[currentStep - 1] : null;

        for (int i = 0; i < movementPerTurn; i++)
        {
            if (currentStep >= path.Count - 1)
                break;

            Tile nextTile = path[currentStep + 1];

            // Ha foglalt és nem Goal, akkor ne menjünk oda
            if ((TileReservationManager.Instance.IsReserved(nextTile) ||
                IsTileOccupiedByEnemy(nextTile)) &&
                nextTile.tileType != TileType.Goal)
            {
                Debug.Log($"{gameObject.name} nem lép, mert {nextTile.gridPosition} foglalt.");
                break;
            }

            // Ne lépjünk vissza!
            if (previousTile != null && nextTile == previousTile)
            {
                Debug.Log($"{gameObject.name} nem lép vissza: {nextTile.gridPosition}");
                break;
            }

            // Lefoglaljuk a célmezőt
            TileReservationManager.Instance.Reserve(nextTile);

            currentStep++;
            currentTile = path[currentStep];

            Vector3 startPos = transform.position;
            Vector3 endPos = currentTile.transform.position + Vector3.up * 0.5f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDelay;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            stepsTaken++;
        }

        // Ha nem lépett egyet sem, próbáljon új utat keresni
        if (stepsTaken == 0)
        {
            Tile from = path[currentStep];
            var newPath = Pathfinder.Instance.FindPath(from.gridPosition, endCoords, behaviorType);

            // Ha az új útvonal nem hátralépéssel indul, akkor elfogadjuk
            if (newPath.Count > 1 && (previousTile == null || newPath[1] != previousTile))
            {
                Debug.Log($"{gameObject.name} újratervezett.");
                path = newPath;
                currentStep = 0;
            }
            else
            {
                Debug.Log($"{gameObject.name} nem talált értelmes új utat (visszalépés lenne).");
            }
        }
    }





    bool IsTileOccupiedByEnemy(Tile tile)
    {
        Collider[] hits = Physics.OverlapSphere(tile.transform.position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<DummyEnemy>() != null && hit.gameObject != gameObject)
            {
                return true; // van rajta másik enemy
            }
        }
        return false;
    }



    void CheckForTrap()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var hit in hits)
        {
            Trap trap = hit.GetComponent<Trap>();
            if (trap != null)
            {
                trap.Trigger(this);
                break;
            }
        }
    }

    public Tile GetProjectedTargetTile()
    {
        int stepsTaken = 0;
        int stepIndex = currentStep;
        Tile target = null;

        for (int i = 0; i < baseMovementPerTurn; i++)
        {
            if (stepIndex >= path.Count - 1)
                break;

            Tile nextTile = path[stepIndex + 1];

            // Csak a Goal mezőn engedélyezett a torlódás
            if (TileReservationManager.Instance.IsReserved(nextTile) &&
                nextTile.tileType != TileType.Goal)
            {
                break;
            }

            stepIndex++;
            target = path[stepIndex];
            stepsTaken++;
        }

        return target;
    }




    public void Die(){
        Instantiate(deathVFX,transform.position + new Vector3(0,1,0),Quaternion.identity);
        Destroy(gameObject);
    }
}
