using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public Slider hpSlider; // assign in inspector

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (hpSlider != null)
            hpSlider.value = 1f;
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

        if (currentHealth <= 0f)
        {
            Debug.Log("Player Died");
            // TODO: trigger lose screen or respawn
        }
    }
}