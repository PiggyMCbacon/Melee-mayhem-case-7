using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    [Tooltip("Seconds required while player is inside to win.")]
    public float requiredSeconds = 120f;

    private float currentSeconds = 0f;
    private bool playerInside = false;

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
            // Optional: expose progress to UI
            // Debug.Log($"Capture progress: {currentSeconds}/{requiredSeconds}");
            if (currentSeconds >= requiredSeconds)
            {
                // win
                GameManager.I.Victory();
                enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}