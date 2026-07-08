using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(PlayerActions))]

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractionDetector : MonoBehaviour
{
    [SerializeField] private float scanInterval = 0.1f;

    private InputAction interactInput;
    private PlayerActions playerActions;
    private PlayerController playerController;


    private readonly HashSet<Interactable> candidates = new();
    private Interactable currentTarget;
    private float scanTimer;

    private void Awake()
    {
        playerActions = GetComponent<PlayerActions>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        interactInput = InputSystem.actions.FindAction("Interact");
        interactInput.Enable();
        interactInput.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactInput.performed -= OnInteract;
        interactInput.Disable();
    }

    // --- TRIGGER DETECTION
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
            candidates.Add(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
            candidates.Remove(interactable);
    }

    // --- Target selection based on proximity (running with an interval)
    private void Update()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer < scanInterval) return;
        scanTimer = 0f;
        RefreshTarget();
    }

    private void RefreshTarget()
    {
        if(playerController.State != PlayerState.Free)
        {
            ClearTarget();
            return;
        }

        // Remove any candidate that might have been destroyed from the candidates set
        candidates.RemoveWhere(c => c == null);

        // Find the closest candidate
        Interactable best = null;
        float closestDist = float.MaxValue;
        foreach (var candidate in candidates)
        {
            float dist = (candidate.transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                best = candidate;
                closestDist = dist;
            }
        }

        if (best == currentTarget) return;

        if (best == null) ClearTarget();
        else SetTarget(best);
    }

    private void SetTarget(Interactable target)
    {
        currentTarget = target;
        InteractionPromptManager.Instance.Show(target);
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
            currentTarget = null;
        InteractionPromptManager.Instance.Hide();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {

        if (currentTarget != null)
        {
            //Debug.Log($"Interacted with: {currentTarget.name}");
            currentTarget.Interact(playerActions);
        }
    }
}
