using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider slider;               // assign in prefab or left null to auto-find
    public Transform target;
    public Vector3 offset = Vector3.up * 2f;

    void Awake()
    {
        // if slider wasn't assigned in inspector, try to find one in children
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogWarning($"[EnemyHealthUI] No Slider assigned or found in children on '{name}'.");
            return;
        }

        // Use normalized 0..1 slider range for simplicity
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
    }

    /// <summary>
    /// Set health using a normalized value 0..1.
    /// </summary>
    public void SetHealth(float normalized)
    {
        if (slider == null) return;
        slider.value = Mathf.Clamp01(normalized);
    }

    /// <summary>
    /// Legacy-compatible: keep but treat max as ignored.
    /// </summary>
    public void SetMaxHealth(int max)
    {
        // intentionally no-op because we use normalized slider
        // kept for compatibility
    }

    public void SetPosition(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        // follow target and face camera
        if (target != null)
        {
            transform.position = target.position + offset;
            if (Camera.main != null)
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}