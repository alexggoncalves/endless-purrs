using System;
using UnityEngine;

public enum AbilityType
{
    Call,
    Home
}

public class PlayerAbilities : MonoBehaviour
{
    public event Action<AbilityType, bool> OnAbilityUsed; //Action(ability,locked)

    // CALL
    [SerializeField] private float callCooldown = 5f;
    [SerializeField] private AudioClip callAudioClip;
    public event Action<float> OnCallCooldown;
    public event Action OnCallReady;
    private float callTimer = 0f;

    // TELEPORT HOME
    public event Action<bool> OnHomeLockChanged;
    private bool homeLocked;

    private void Update()
    {
        UpdateCallAbility();
    }

    public void UseAbility(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Call:
                HandleCallCats();
                break;
            case AbilityType.Home:
                HandleTeleportHome();
                break;
            default: 
                break;
        }
    }

    private void HandleCallCats()
    {
        // If ability locked invoke ability use with locked = true
        if (callTimer > 0f)
        {
            OnAbilityUsed?.Invoke(AbilityType.Call, true);
            return;
        }
           
        // Play sound and invoke event for the HUDController
        SoundFXManager.Instance.PlaySoundFXClip(callAudioClip, transform.position, 1);
        OnAbilityUsed?.Invoke(AbilityType.Call, false);

        // Start Cooldown
        callTimer = callCooldown;
        OnCallCooldown?.Invoke(0f);
    }

    private void HandleTeleportHome()
    {
        OnAbilityUsed(AbilityType.Home, !homeLocked);
    }

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

    public void SetHomeLocked(bool locked)
    {
        if (homeLocked == locked)
            return;

        homeLocked = locked;
        OnHomeLockChanged?.Invoke(homeLocked);
    }
}

