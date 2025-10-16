using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveSetting
{
    [Range(0f, 1f)] public float progressThreshold; // 0.2 = 20% capture progress
    public int maxEnemies;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn Behavior")]
    public float checkInterval = 1f;
    [Tooltip("How many enemies can spawn per check interval.")]
    public int spawnPerCheck = 5;

    [Header("Wave Settings")]
    [Tooltip("Define how many enemies can exist at different capture progress points.")]
    public List<WaveSetting> waveSettings = new List<WaveSetting>
    {
        new WaveSetting { progressThreshold = 0f, maxEnemies = 1 },
        new WaveSetting { progressThreshold = 0.4f, maxEnemies = 2 },
        new WaveSetting { progressThreshold = 0.6f, maxEnemies = 4 },
        new WaveSetting { progressThreshold = 0.8f, maxEnemies = 8 },
        new WaveSetting { progressThreshold = 0.8f, maxEnemies = 10 },
    };

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnRoutine;
    private int currentMaxEnemies = 0;

    void Start()
    {
        // Sort the list just in case
        waveSettings.Sort((a, b) => a.progressThreshold.CompareTo(b.progressThreshold));
    }

    public void BeginSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
            Debug.Log("[EnemySpawner] Spawning started.");
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
            Debug.Log("[EnemySpawner] Spawning stopped.");
        }
    }

    public void SetProgress(float normalizedProgress)
    {
        // normalizedProgress = currentSeconds / requiredSeconds (0–1)
        foreach (var wave in waveSettings)
        {
            if (normalizedProgress >= wave.progressThreshold)
                currentMaxEnemies = wave.maxEnemies;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            CleanNulls();

            int allowed = Mathf.Max(currentMaxEnemies - activeEnemies.Count, 0);
            int toSpawn = Mathf.Clamp(allowed, 0, spawnPerCheck);

            for (int i = 0; i < toSpawn; i++)
                SpawnOne();
        }
    }

    void SpawnOne()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] Missing prefab or spawn points.");
            return;
        }

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject e = Instantiate(enemyPrefab, sp.position, sp.rotation);
        activeEnemies.Add(e);

        var enemyAI = e.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.onDie += () =>
            {
                activeEnemies.Remove(e);
            };
        }
    }

    void CleanNulls()
    {
        activeEnemies.RemoveAll(x => x == null);
    }

    public int ActiveCount() => activeEnemies.Count;
}