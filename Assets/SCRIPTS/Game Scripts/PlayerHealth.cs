using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 10f;
    public Slider hpSlider; // assign in inspector

    [Header("Fade to Defeat")]
    public Image fadeOverlay; // full screen black Image, assign in inspector
    public float fadeDuration = 1f;

    [Header("Damage Flash")]
    public Image damageFlash; // full screen red Image, assign in inspector
    public float flashDuration = 0.2f; // how fast the flash fades

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (hpSlider != null)
            hpSlider.value = 1f;

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(0f, 0f, 0f, 0f); // start transparent
        }

        if (damageFlash != null)
        {
            damageFlash.gameObject.SetActive(true);
            damageFlash.color = new Color(1f, 0f, 0f, 0f); // start transparent
        }
    }

    public void TakeDamage(float amount, Vector3 knockback)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (hpSlider != null)
            hpSlider.value = currentHealth / maxHealth;

        // optional: knockback
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(knockback, ForceMode.Impulse);

        // Flash red outline
        if (damageFlash != null)
            StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0f)
        {
            Debug.Log("Player Died");
            StartCoroutine(FadeToDefeat());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        Color c = damageFlash.color;
        c.a = 0.7f; // set initial alpha for flash
        damageFlash.color = c;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.7f, 0f, elapsed / flashDuration);
            damageFlash.color = c;
            yield return null;
        }

        c.a = 0f;
        damageFlash.color = c;
    }

    private IEnumerator FadeToDefeat()
    {
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                fadeOverlay.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeOverlay.color = c;
        }

        // After fade is complete, load defeat scene
        SceneManager.LoadScene("DefeatScene");
    }
}