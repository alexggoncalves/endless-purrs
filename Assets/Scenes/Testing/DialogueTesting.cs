using UnityEngine;

public class DialogueTesting : MonoBehaviour
{

    [SerializeField] private DialogueData initialDialogue;
    [SerializeField] private Transform anchor;


    void Start()
    {
        DialogueManager.Instance.StartDialogue(initialDialogue, anchor, OnDialogueEnd);
    }


    private void OnDialogueEnd(string a)
    {
        Debug.Log("Dialogue over " + a);
    }
}
