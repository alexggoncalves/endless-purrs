using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScreenFadeController : MonoBehaviour
{
    [SerializeField] private Color overlayColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float initialAlpha = 0f;

    private RawImage overlay;

    private void Start()
    {
        overlay = GetComponent<RawImage>();
        overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, initialAlpha);
    }

    /// <summary>
    /// Fades in scene (Fade out overlay)
    /// </summary>
    public IEnumerator FadeIn(float transitionDuration)
    {
        yield return FadeOverlay(1f, 0f, transitionDuration);
    }

    /// <summary>
    /// Fades out scene (Fade in overlay)
    /// </summary>
    public IEnumerator FadeOut(float transitionDuration)
    {
        yield return FadeOverlay(0f, 1f, transitionDuration);
    }

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color baseColor = overlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float alpha = Mathf.Lerp(from, to, smoothT);


            overlay.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        overlay.color = new Color(baseColor.r, baseColor.g, baseColor.b, to);
    }
}
