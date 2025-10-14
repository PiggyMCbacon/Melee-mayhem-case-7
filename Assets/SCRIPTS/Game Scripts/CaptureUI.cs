using UnityEngine;
using UnityEngine.UI;

public class CaptureUI : MonoBehaviour
{
    public Slider captureSlider;
    public GameObject captureContainer; // optional parent panel to toggle visibility

    private CapturePoint capturePoint;

    void Start()
    {
        // find capture point in the scene
        capturePoint = FindObjectOfType<CapturePoint>();
        if (capturePoint != null)
        {
            capturePoint.onCaptureProgress += UpdateProgress;
            capturePoint.onCaptureEnter += ShowUI;
            capturePoint.onCaptureExit += HideUI;
        }

        if (captureContainer != null)
            captureContainer.SetActive(false);
    }

    void UpdateProgress(float current, float required)
    {
        if (captureSlider != null)
        {
            captureSlider.value = current / required;
        }
    }

    void ShowUI()
    {
        if (captureContainer != null)
            captureContainer.SetActive(true);
    }

    void HideUI()
    {
        if (captureContainer != null)
            captureContainer.SetActive(false);
    }
}