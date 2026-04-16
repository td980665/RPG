using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string id;

    public DialogueRunner.NodeType type;

    [TextArea]
    public string[] lines;

    public string nextNodeId;

    public List<Choice> choices;
    public List<Branch> branches;

    public List<Condition> conditions;
    public List<Effect> effects;
}