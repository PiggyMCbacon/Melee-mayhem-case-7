using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    public Slider slider;
    public Transform target;
    public Vector3 offset = Vector3.up * 2f;

    public void SetMaxHealth(int max)
    {
        if (slider != null)
            slider.maxValue = max;
    }

    public void SetHealth(float normalized)
    {
        if (slider != null)
            slider.value = normalized * slider.maxValue;
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