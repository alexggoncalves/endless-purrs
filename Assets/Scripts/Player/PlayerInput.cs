using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(PlayerAbilities))]
[RequireComponent(typeof(PlayerActions))]

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private GameManager game;

    private PlayerActions playerActions;
    private PlayerAbilities playerAbilities;

    private InputAction callCatsAction;
    private InputAction teleportAction;
    private InputAction interactAction;

    private void Start()
    {
        playerActions = GetComponent<PlayerActions>();
        playerAbilities = GetComponent<PlayerAbilities>();

        callCatsAction = InputSystem.actions.FindAction("CallCats");
        teleportAction = InputSystem.actions.FindAction("TeleportHome");
        interactAction = InputSystem.actions.FindAction("Interact");
    }


    void Update()
    {
        HandleAbilities();
        HandleActions();
    }

    private void HandleAbilities()
    {
        if (callCatsAction.IsPressed())
            playerAbilities.UseAbility(AbilityType.Call);

        if (teleportAction.IsPressed())
            playerAbilities.UseAbility(AbilityType.Home);
    }

    private void HandleActions()
    {
        if (interactAction.IsPressed())
        {
            playerActions.TryPetCat();
        }
    }
}
