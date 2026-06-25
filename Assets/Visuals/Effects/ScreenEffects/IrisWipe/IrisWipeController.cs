using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class IrisWipeController : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new(0,0);
    [SerializeField] private float softness = 0.2f;

    private readonly float closedRadius = -0.2f;
    private readonly float openedRadius = 1.2f;

    private RawImage irisImage;
    private Material irisMaterial;

    private void Awake()
    {
        irisImage = GetComponent<RawImage>();
        irisMaterial = new Material(irisImage.material);
        irisImage.material = irisMaterial;
        irisMaterial.SetFloat("_Radius", openedRadius);
        irisMaterial.SetVector("_Offset", offset);
        irisMaterial.SetFloat("_Softness", softness);
    }

    /// <summary>
    /// Closes the iris from outside in
    /// </summary>
    public IEnumerator CloseIris(float transitionDuration)
    {
        yield return ScaleIris(openedRadius, closedRadius, transitionDuration);
    }

    /// <summary>
    /// Opens the iris from center out
    /// </summary>
    public IEnumerator OpenIris(float transitionDuration)
    {
        yield return ScaleIris(closedRadius, openedRadius, transitionDuration);
    }

    public void SetClosed()
    {
        irisMaterial.SetFloat("_Radius", closedRadius);
    }

    public void setOpened()
    {
        irisMaterial.SetFloat("_Radius", openedRadius);
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