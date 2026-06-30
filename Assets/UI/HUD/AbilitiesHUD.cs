using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AbilitiesHUD : MonoBehaviour
{
    [SerializeField] private PlayerAbilities playerAbilities;

    private VisualElement root;
    private VisualElement callAbility;
    private VisualElement callCooldown;
    private VisualElement homeAbility;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        callAbility = root.Q<VisualElement>("CallAbility");
        callCooldown = root.Q<VisualElement>("CallCooldown");
        homeAbility = root.Q<VisualElement>("HomeAbility");

        playerAbilities.OnAbilityUsed += OnAbilityUsed;
        playerAbilities.OnCallCooldown += OnCallCooldown;
        playerAbilities.OnCallReady += OnCallReady;
        playerAbilities.OnHomeLockChanged += OnHomeLockedChange;

    }

    private void OnDisable()
    {
        playerAbilities.OnAbilityUsed -= OnAbilityUsed;
        playerAbilities.OnCallCooldown -= OnCallCooldown;
        playerAbilities.OnCallReady -= OnCallReady;
        playerAbilities.OnHomeLockChanged -= OnHomeLockedChange;
    }

    private void OnAbilityUsed(AbilityType type, bool locked)
    {
        if (type == AbilityType.Call)
        {
            Pulse(callAbility, locked);
            callAbility.AddToClassList("ability-locked");
        }

        if (type == AbilityType.Home)
        {
            Pulse(homeAbility, locked);
        }
    }

    void OnCallCooldown(float progress)
    {
        callCooldown.style.width = Length.Percent(100f - (progress * 100f));
    }

    void OnCallReady()
    {
        callAbility.RemoveFromClassList("ability-locked");
        callCooldown.style.width = 0;
    }
    
    /// <summary>
    /// Update home ability locked visual state
    /// </summary>
    void OnHomeLockedChange(bool locked)
    {
        if (locked)
            homeAbility.AddToClassList("ability-locked");
        else
            homeAbility.RemoveFromClassList("ability-locked");
    }

    /// <summary>
    /// Pulse ability visual element (scale up and down)
    /// </summary>
    private void Pulse(VisualElement element, bool active)
    {
        element.experimental.animation.Scale(1.05f, 120).OnCompleted(() =>
        {
            element.experimental.animation.Scale(1f, 200);
        });

        if (!active)
        {
            // Apply red filter
        }
    }
}
