using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private readonly List<Transform> enemies = new List<Transform>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterEnemy(Transform enemy)
    {
        if (enemy != null && !enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (enemy != null)
            enemies.Remove(enemy);
    }

    public Transform GetClosestEnemy(Transform from)
    {
        if (from == null || enemies.Count == 0)
            return null;

        Transform closest = null;
        float minDistSq = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            float dSq = (e.position - from.position).sqrMagnitude;
            if (dSq < minDistSq)
            {
                minDistSq = dSq;
                closest = e;
            }
        }

        return closest;
    }
}
