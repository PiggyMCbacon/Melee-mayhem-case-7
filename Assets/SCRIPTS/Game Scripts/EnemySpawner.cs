using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float checkInterval = 5f;

    [Tooltip("Prevent too many spawns at once.")]
    public int spawnPerCheck = 1;

    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            CleanNulls();

            int allowed = GameManager.I != null ? GameManager.I.AllowedActiveEnemies() : 1;
            int toSpawn = Mathf.Clamp(allowed - activeEnemies.Count, 0, spawnPerCheck);

            for (int i = 0; i < toSpawn; i++)
            {
                SpawnOne();
            }
        }
    }

    void SpawnOne()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject e = Instantiate(enemyPrefab, sp.position, sp.rotation);
        activeEnemies.Add(e);

        // hook to removal on death
        var death = e.GetComponent<EnemyAI>();
        if (death != null) death.onDie += () => { activeEnemies.Remove(e); };
    }

    void CleanNulls()
    {
        activeEnemies.RemoveAll(x => x == null);
    }

    // Optional: for debugging
    public int ActiveCount() => activeEnemies.Count;
}