using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class CapturePoint : MonoBehaviour
{
    [Header("Capture Settings")]
    public float requiredSeconds = 120f;
    private float currentSeconds = 0f;
    private bool playerInside = false;

    // ✅ UI Events
    public event Action<float, float> onCaptureProgress;
    public event Action onCaptureEnter;
    public event Action onCaptureExit;

    private GameManager gameManager;

    private void Awake()
    {
        // Make sure the collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Try to find GameManager safely
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        // Load requiredSeconds from GameManager if available
        if (gameManager != null)
            requiredSeconds = gameManager.captureRequiredSeconds;
    }

    private void Update()
    {
        if (playerInside)
        {
            currentSeconds += Time.deltaTime;
            onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

            // Clamp and handle victory
            if (currentSeconds >= requiredSeconds)
            {
                currentSeconds = requiredSeconds;
                onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);

                if (gameManager != null)
                    gameManager.Victory();
                else
                    Debug.LogWarning("GameManager not found — victory not triggered.");

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

            // Optional: reset progress if the player leaves
            // currentSeconds = 0f;
            // onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);
        }
    }

    // Optional: expose a reset for replays/testing
    public void ResetCapture()
    {
        playerInside = false;
        currentSeconds = 0f;
        onCaptureProgress?.Invoke(currentSeconds, requiredSeconds);
    }
}