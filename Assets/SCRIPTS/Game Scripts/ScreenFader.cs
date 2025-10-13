using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image blackImage; // UI Image covering the screen. Alpha 0 initially

    private void Start()
    {
        if (blackImage != null)
        {
            var c = blackImage.color;
            c.a = 0f;
            blackImage.color = c;
        }
    }

    public IEnumerator FadeToBlack(float duration)
    {
        if (blackImage == null) yield break;
        float t = 0f;
        Color c = blackImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration);
            blackImage.color = c;
            yield return null;
        }
    }
}