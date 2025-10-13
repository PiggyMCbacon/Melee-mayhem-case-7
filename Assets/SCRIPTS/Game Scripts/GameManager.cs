using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Capture")]
    public float captureRequiredSeconds = 120f;
    public string victorySceneName = "VictoryScene";

    [Header("Spawner")]
    public float matchElapsedTime { get; private set; } = 0f;

    private void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        matchElapsedTime += Time.deltaTime;
    }

    public int AllowedActiveEnemies()
    {
        // first 20s => 1 active. then +1 every 20 seconds
        // floor(matchElapsedTime / 20)
        int extra = Mathf.FloorToInt(matchElapsedTime / 20f);
        return 1 + extra;
    }

    public void Victory()
    {
        // Called by CapturePoint when player reaches captureRequiredSeconds
        StartCoroutine(EndAndLoadVictory());
    }

    System.Collections.IEnumerator EndAndLoadVictory()
    {
        // fade handled by ScreenFader
        var fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
        {
            yield return fader.FadeToBlack(2f); // fade duration 2s
        }

        // load victory scene
        if (!string.IsNullOrEmpty(victorySceneName))
            SceneManager.LoadScene(victorySceneName);
    }
}