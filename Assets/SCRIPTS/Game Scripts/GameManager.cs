using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Capture Settings")]
    [Tooltip("Seconds required to hold the capture point for victory.")]
    public float captureRequiredSeconds = 120f;

    [Tooltip("Name of the victory scene to load after capture.")]
    public string victorySceneName = "VictoryScene";

    [Header("Spawner Control")]
    [Tooltip("Tracks how long the match has been running.")]
    public float matchElapsedTime { get; private set; } = 0f;

    private bool victoryTriggered = false;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        matchElapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Calculates how many enemies should be allowed active based on elapsed time.
    /// </summary>
    public int AllowedActiveEnemies()
    {
        // First 20 seconds → 1 active enemy
        // Every additional 20 seconds → +1
        int extra = Mathf.FloorToInt(matchElapsedTime / 20f);
        return Mathf.Clamp(1 + extra, 1, 10); // clamp max if needed
    }

    /// <summary>
    /// Called by CapturePoint when the player wins.
    /// </summary>
    public void Victory()
    {
        if (victoryTriggered) return;
        victoryTriggered = true;
        StartCoroutine(EndAndLoadVictory());
    }

    private IEnumerator EndAndLoadVictory()
    {
        // Try to fade out if a fader exists
        var fader = FindFirstObjectByType<ScreenFader>();
        if (fader != null)
        {
            yield return fader.FadeToBlack(2f); // fade duration 2s
        }
        else
        {
            Debug.LogWarning("ScreenFader not found — skipping fade.");
            yield return new WaitForSeconds(2f);
        }

        // Load victory scene safely
        if (!string.IsNullOrEmpty(victorySceneName))
        {
            SceneManager.LoadScene(victorySceneName);
        }
        else
        {
            Debug.LogError("Victory scene name not set in GameManager!");
        }
    }
}