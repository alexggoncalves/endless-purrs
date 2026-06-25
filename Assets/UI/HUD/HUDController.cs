using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement hud;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        hud = root.Q<VisualElement>("HUD");

        GameManager.Instance.RegisterHUD(this);
    }
    public void Hide()
    {
        hud.style.opacity = 0f;
    }

    public IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            hud.style.opacity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        hud.style.opacity = to;
    }
}
