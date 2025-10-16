using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DefeatScreenButtons : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameplaySceneName = "GameScene"; // your main gameplay scene
    public string mainMenuSceneName = "MainMenu";  // your main menu scene

    [Header("Fade Settings")]
    public Image fadeImage;       // assign a full-screen black UI Image
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Make sure the fade starts transparent
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 0); 
        }

        // Unlock and show the cursor so buttons are clickable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Restarts the gameplay scene from scratch with fade.
    /// </summary>
    public void OnRestartButton()
    {
        if (fadeImage != null)
            StartCoroutine(FadeAndLoadScene(gameplaySceneName, lockCursor: true));
        else
            LoadGameplayScene();
    }

    /// <summary>
    /// Loads the main menu scene with fade.
    /// </summary>
    public void OnMainMenuButton()
    {
        if (fadeImage != null)
            StartCoroutine(FadeAndLoadScene(mainMenuSceneName, lockCursor: false));
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator FadeAndLoadScene(string sceneName, bool lockCursor)
    {
        float timer = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        fadeImage.color = endColor;

        // Lock cursor if loading gameplay
        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void LoadGameplayScene()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(gameplaySceneName);
    }
}