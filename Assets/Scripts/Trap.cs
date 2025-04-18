using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public GameObject hitVFX;

    public enum TrapType
    {
        Exploding,
        Ice,
        Poison,
        Burn,
        Shock,
        Bleed
    }

    public TrapType trapType = TrapType.Exploding;

    public void Trigger(DummyEnemy enemy)
    {
        Debug.Log("Csapda aktiválódik: " + gameObject.name);

        switch (trapType)
        {
            case TrapType.Exploding:
                Debug.Log("BOOM");
                Explode(enemy);
                break;

            case TrapType.Ice:
                Debug.Log("JÉGSUGÁÁR");
                Freeze(enemy,1);
                break;

            default:
                Debug.LogWarning("Ismeretlen csapda típus: " + trapType);
                break;
        }

        Destroy(gameObject);
    }

    public void Explode(DummyEnemy enemy)
    {
        Instantiate(hitVFX, transform.position, Quaternion.identity);
        enemy.TakeDamage(20);
    }

    public void Freeze(DummyEnemy enemy, int turns)
    {
        Instantiate(hitVFX, transform.position, Quaternion.identity);
        
        //enemy.isFrozen = true;
        enemy.frozenTurnsRemaining = turns;
        Slow(enemy, 2);
    }

    public void Slow(DummyEnemy enemy, int turns)
    {
        enemy.isSlowed = true;
        enemy.slowedTurnsRemaining = turns;
    }
}
