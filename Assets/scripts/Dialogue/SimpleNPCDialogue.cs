using UnityEngine;

public class SimpleNPCDialogue : MonoBehaviour, IInteractable
{
    public DialogueData firstDialogue;
    public string firstNodeId = "start";

    public DialogueData repeatDialogue;
    public string repeatNodeId = "start";

    public string talkedFlag;

    public void Interact()
    {
        Debug.Log("Interact called");

        var runner = FindFirstObjectByType<DialogueRunner>();

        bool talked = GameFlagManager.Instance.GetBool(talkedFlag);

        if (!talked)
        {
            Debug.Log("First dialogue");

            runner.StartDialogue(firstDialogue, firstNodeId);

            GameFlagManager.Instance.SetBool(talkedFlag, true);
        }
        else
        {
            Debug.Log("Repeat dialogue");

            runner.StartDialogue(repeatDialogue, repeatNodeId);
        }
    }
}