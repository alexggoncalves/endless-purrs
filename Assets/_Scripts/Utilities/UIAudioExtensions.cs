using UnityEngine;
using UnityEngine.UIElements;

public static class UIAudioExtensions
{
    public static T WithClickSound<T>(this T element, AudioClip clip) where T : VisualElement
    {
        element.RegisterCallback<ClickEvent>(_ => SoundFXManager.Instance.Play2DSoundFXClip(clip, 0.05f));
        return element;
    }

    public static T WithHoverSound<T>(this T element, AudioClip clip) where T : VisualElement
    {
        element.RegisterCallback<PointerEnterEvent>(_ => SoundFXManager.Instance.Play2DSoundFXClip(clip, 0.05f));
        return element;
    }
}