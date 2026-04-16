using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes;

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