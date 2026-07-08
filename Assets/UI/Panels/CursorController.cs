using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CursorController : PersistentSingleton<CursorController>
{
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private VisualElement cursor;

    void OnEnable()
    {
        UnityEngine.Cursor.visible = false;
        cursor = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Cursor");
    }

    void OnDisable() => UnityEngine.Cursor.visible = true;

    void LateUpdate()
    {
        if (cursor?.panel == null || Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        bool inside = screenPos.x >= 0 && screenPos.x < Screen.width
                   && screenPos.y >= 0 && screenPos.y < Screen.height;

        cursor.style.display = inside ? DisplayStyle.Flex : DisplayStyle.None;
        if (!inside) return;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(cursor.panel, screenPos);
        float panelHeight = cursor.panel.visualTree.layout.height;

        cursor.style.left = panelPos.x - hotspot.x;
        cursor.style.top = (panelHeight - panelPos.y) - hotspot.y;
    }
}
