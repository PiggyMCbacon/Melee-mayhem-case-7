using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer; // assign your GameAudioMixer

    public static bool GameIsPaused = false;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    void Start()
    {
        pausePanel.SetActive(false);

        // Setup default values (1.0 = full volume)
        if (musicSlider != null)
        {
            musicSlider.value = 1f;
            musicSlider.onValueChanged.AddListener(AdjustMusic);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
            sfxSlider.onValueChanged.AddListener(AdjustSFX);
        }

        // Initialize mixer values
        AdjustMusic(musicSlider.value);
        AdjustSFX(sfxSlider.value);

        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 🧩 Ensure any weird time scaling is fixed
        if (Mathf.Abs(Time.timeScale - 1f) > 0.001f)
            Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    // Volume adjustments
    public void AdjustMusic(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(MUSIC_PARAM, volume);
    }

    public void AdjustSFX(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(SFX_PARAM, volume);
    }
}