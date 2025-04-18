using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int enemiesEscaped = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterSuccess()
    {
        enemiesEscaped++;
        Debug.Log("Ellenség bejutott! Jelenlegi: " + enemiesEscaped);
    }

    public void RegisterMultiple(int count)
    {
        enemiesEscaped += count;
        Debug.Log($"+{count} ellenség bejutott! Összesen: {enemiesEscaped}");
    }
}
