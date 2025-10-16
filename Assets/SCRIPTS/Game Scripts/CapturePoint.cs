using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class CapturePoint : MonoBehaviour
{
    [Header("Capture Settings")]
    public float requiredSeconds = 120f;
    private float currentSeconds = 0f;
    private bool playerInside = false;

    [Header("Spawner Link")]
    [Tooltip("Assign the EnemySpawner that should react to capture progress.")]
    public EnemySpawner enemySpawner;

    // ✅ UI Events
    public event Action<float, float> onCaptureProgress;
    public event Action onCaptureEnter;
    public event Action onCaptureExit;

    private GameManager gameManager;

    private void Awake()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Find the GameManager if present
        gameManager = FindFirstObjectByType<GameManager>();

        // Ensure spawner isn't running at start
        if (enemySpawner != null)
            enemySpawner.StopSpawning();
    }

    private void Start()
    {
        // load configured capture time from GameManager if available
        if (gameManager != null)
            requiredSeconds = gameManager.captureRequiredSeconds;
    }

    private void Update()
    {
        if (!playerInside) return;

        currentSeconds += Time.deltaTime;
        onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

        // report normalized progress (0..1) to the spawner so it can pick the configured wave
        float progress = Mathf.Clamp01(requiredSeconds <= 0f ? 0f : (currentSeconds / requiredSeconds));
        if (enemySpawner != null)
            enemySpawner.SetProgress(progress);

        // Victory check
        if (currentSeconds >= requiredSeconds)
        {
            currentSeconds = requiredSeconds;
            onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

            if (gameManager != null)
                gameManager.Victory();
            else
                Debug.LogWarning("[CapturePoint] GameManager not found — victory not triggered.");

            // stop spawner when captured
            if (enemySpawner != null)
                enemySpawner.StopSpawning();

            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        onCaptureEnter?.Invoke();

        if (enemySpawner != null)
            enemySpawner.BeginSpawning();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        onCaptureExit?.Invoke();

        if (enemySpawner != null)
            enemySpawner.StopSpawning();
    }

    // Reset capture for testing / replay
    public void ResetCapture()
    {
        playerInside = false;
        currentSeconds = 0f;
        onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
            enemySpawner.SetProgress(0f);
        }

        enabled = true;
    }
}