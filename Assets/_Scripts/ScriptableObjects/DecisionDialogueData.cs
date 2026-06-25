using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueData nextDialogue; 
    public string resultId;
}


[CreateAssetMenu(fileName = "Decision", menuName = "Scriptable Objects/Decision")]
public class DecisionDialogueData : DialogueData
{
    public DialogueChoice choiceA;
    public DialogueChoice choiceB;
}

