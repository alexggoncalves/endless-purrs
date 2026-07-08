using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private string promptText = "Pet";
    [SerializeField] private Vector3 promptOffset = new(0f, 1.5f, 0f);

    public string PromptText => promptText;
    public Vector3 PromptWorldPosition => transform.position + promptOffset;

    public virtual void Interact(PlayerActions player) { }
}
