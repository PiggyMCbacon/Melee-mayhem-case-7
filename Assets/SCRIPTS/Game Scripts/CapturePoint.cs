using UnityEngine;
using System;

public class CapturePoint : MonoBehaviour
{
    public float requiredSeconds = 120f;
    private float currentSeconds = 0f;
    private bool playerInside = false;

    // Events for UI
    public event Action<float, float> onCaptureProgress;
    public event Action onCaptureEnter;
    public event Action onCaptureExit;

    private void Start()
    {
        if (GameManager.I != null)
            requiredSeconds = GameManager.I.captureRequiredSeconds;
    }

    private void Update()
    {
        if (playerInside)
        {
            currentSeconds += Time.deltaTime;
            onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

            if (currentSeconds >= requiredSeconds)
            {
                GameManager.I.Victory();
                enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            onCaptureEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            onCaptureExit?.Invoke();
        }
    }
}