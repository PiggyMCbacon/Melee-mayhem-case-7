using UnityEngine;
using UnityEngine.UI;

public class CaptureUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider captureSlider;
    public GameObject captureContainer; // Optional parent panel to toggle visibility

    [Header("Capture System References")]
    public CapturePoint capturePoint;   // Can assign manually in Inspector

    private PlayerMovement player;

    void Start()
    {
        // Hide UI initially
        if (captureContainer != null)
            captureContainer.SetActive(false);

        // Find player automatically
        player = FindFirstObjectByType<PlayerMovement>();

        // Find capture point automatically if not assigned
        if (capturePoint == null)
            capturePoint = FindFirstObjectByType<CapturePoint>();

        // Subscribe to capture point events if available
        if (capturePoint != null)
        {
            capturePoint.onCaptureProgress += UpdateProgress;
            capturePoint.onCaptureEnter += ShowUI;
            capturePoint.onCaptureExit += HideUI;
        }
        else
        {
            Debug.LogWarning("No CapturePoint found in the scene for CaptureUI!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (capturePoint != null)
        {
            capturePoint.onCaptureProgress -= UpdateProgress;
            capturePoint.onCaptureEnter -= ShowUI;
            capturePoint.onCaptureExit -= HideUI;
        }
    }

    private void UpdateProgress(float current, float required)
    {
        if (captureSlider != null)
        {
            // Only update the slider when player is inside the zone
            captureSlider.value = Mathf.Clamp01(current / required);
        }
    }

    private void ShowUI()
    {
        if (captureContainer != null)
            captureContainer.SetActive(true);
    }

    private void HideUI()
    {
        if (captureContainer != null)
            captureContainer.SetActive(false);
    }
}