using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)] public string text;
    public AudioClip voiceClip; // optional
}

public abstract class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

}

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class NormalDialogueData : DialogueData
{
    public DecisionDialogueData followingDecision;
}
