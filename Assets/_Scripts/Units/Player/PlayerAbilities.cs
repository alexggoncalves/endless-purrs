using System;
using UnityEngine;

public enum AbilityType
{
    Call,
    Home
}

public class PlayerAbilities : MonoBehaviour
{
    public event Action<AbilityType> OnAbilityUsed;
    public event Action<float> OnCallCooldown;
    public event Action OnCallReady;
    public event Action<bool> OnHomeLockChanged;

    [SerializeField] private float callCooldown = 5f;

    private float callTimer = 0f;
    private bool homeLocked;

    private void Update()
    {
        UpdateCallAbility();
    }

    public void UseAbility(AbilityType type)
    {
        if (type == AbilityType.Call)
        {
            if (callTimer > 0f)
                return;

            callTimer = callCooldown;
            OnAbilityUsed(type);
            OnCallCooldown?.Invoke(0f);
        } else if (type == AbilityType.Home)
        {
            if (homeLocked) return;

            OnAbilityUsed(type);
        }
    }
    
    // CALL
    private void UpdateCallAbility()
    {
        if (callTimer <= 0f) return;

        callTimer -= Time.deltaTime;

        float cooldownProgress = 1f - (callTimer / callCooldown);
        OnCallCooldown?.Invoke(cooldownProgress);

        if (callTimer <= 0f)
        {
            callTimer = 0f;
            OnCallReady?.Invoke();
        }

    }

    // HOME
    public void SetHomeLocked(bool locked)
    {
        if (homeLocked == locked)
            return;

        homeLocked = locked;
        OnHomeLockChanged?.Invoke(homeLocked);
    }
}

