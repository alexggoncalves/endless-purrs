using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class TeleportTransition : MonoBehaviour
{
    
    [SerializeField] private float transitionDuration = 0.4f;

    private readonly float closedRadius = -0.2f;
    private readonly float openedRadius = 1.2f;

    private RawImage irisImage;
    private Material irisMaterial;

    private void Start()
    {
        irisImage = GetComponent<RawImage>();
        irisMaterial = new Material(irisImage.material);
        irisImage.material = irisMaterial;
        irisMaterial.SetFloat("_Radius", openedRadius);
    }

    /// <summary>
    /// Closes the iris from outside in. Awaitable from another coroutine.
    /// </summary>
    public IEnumerator CloseIris()
    {
        yield return ScaleIris(openedRadius, closedRadius, transitionDuration);
    }

    /// <summary>
    /// Opens the iris from center out. Awaitable from another coroutine.
    /// </summary>
    public IEnumerator OpenIris()
    {
        yield return ScaleIris(closedRadius, openedRadius, transitionDuration);
    }

    private IEnumerator ScaleIris(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            irisMaterial.SetFloat("_Radius", Mathf.Lerp(from, to, smoothT));
            yield return null;
        }

        irisMaterial.SetFloat("_Radius", to);
    }
}