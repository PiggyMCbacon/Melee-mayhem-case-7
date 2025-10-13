using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject howToPlayPanel;

    private void Start()
    {
        // Make sure the panel starts hidden
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // Called when Start button is clicked
    public void StartGame()
    {
        // Replace "GameScene" with your scene name
        SceneManager.LoadScene("GameScene");
    }

    // Called when Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // When mouse hovers over How To Play button
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    // When mouse exits the How To Play button
    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }
}