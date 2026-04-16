using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string id;
    [TextArea] public string[] lines;

    public List<Condition> conditions = new List<Condition>();
    public List<Effect> effects = new List<Effect>();

    public List<Choice> choices = new List<Choice>();
    public string nextNodeId;
}

[System.Serializable]
public class Choice
{
    public string text;
    public string nextNodeId;

    public List<Condition> conditions = new List<Condition>();
    public List<Effect> effects = new List<Effect>();
}

[System.Serializable]
public class Condition
{
    public string key;
    public bool value;
}

[System.Serializable]
public class Effect
{
    public string key;
    public bool value;
}

[CreateAssetMenu(menuName = "RPG/Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes;

    // ★ここに入れる
    void OnValidate()
    {
        if (nodes == null) return;

        var idSet = new HashSet<string>();

        foreach (var node in nodes)
        {
            if (node == null) continue;

            if (string.IsNullOrEmpty(node.id))
                Debug.LogError("ID未設定のノードがあります", this);

            if (!idSet.Add(node.id))
                Debug.LogError("ID重複: " + node.id, this);

            if (!string.IsNullOrEmpty(node.nextNodeId) &&
                !nodes.Exists(n => n.id == node.nextNodeId))
            {
                Debug.LogError("存在しないnextNodeId: " + node.nextNodeId, this);
            }

            if (node.choices != null)
            {
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.nextNodeId) &&
                        !nodes.Exists(n => n.id == choice.nextNodeId))
                    {
                        Debug.LogError("ChoiceのnextNodeId不正: " + choice.nextNodeId, this);
                    }
                }
            }
        }
    }
}