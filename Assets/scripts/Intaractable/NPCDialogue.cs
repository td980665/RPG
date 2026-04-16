using UnityEngine;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour, IInteractable
{

    public DialogueRunner runner;

    // ▼ Asset
    public DialogueData dialogueData;
    public string startNodeId;

    // ▼ インライン（Node形式）
    public List<DialogueNode> inlineNodes;

    void Reset()
    {
        inlineNodes = new List<DialogueNode>
    {
        new DialogueNode
        {
            id = "start",
            type = DialogueRunner.NodeType.Line,
            lines = new[] { "こんにちは" },
            nextNodeId = "end",
            conditions = new List<Condition>(),
            effects = new List<Effect>()
        },

        new DialogueNode
        {
            id = "end",
            type = DialogueRunner.NodeType.Line,
            lines = new[] { "またね" },
            nextNodeId = "",
            conditions = new List<Condition>(),
            effects = new List<Effect>()
        }
    };
    }

    public void Interact()
    {
        // ▼ インライン優先
        if (inlineNodes != null && inlineNodes.Count > 0)
        {
            runner.StartDialogue(inlineNodes, "start");
            return;
        }

        // ▼ Asset
        if (dialogueData != null)
        {
            runner.StartDialogue(dialogueData, startNodeId);
        }
        else
        {
            Debug.LogError("Dialogueが未設定", this);
        }
    }

    void OnValidate()
    {
        // ▼ inlineチェック
        if (inlineNodes != null && inlineNodes.Count > 0)
        {
            bool hasStart = inlineNodes.Exists(n => n.id == "start");
            if (!hasStart)
            {
                Debug.LogError("inlineNodesにstartノードがありません", this);
            }
        }

        // ▼ dataチェック
        if (dialogueData != null)
        {
            bool hasStart = dialogueData.nodes.Exists(n => n.id == startNodeId);
            if (!hasStart)
            {
                Debug.LogError("startNodeIdが不正: " + startNodeId, this);
            }
        }

        // ▼ 両方設定警告
        if (inlineNodes != null && inlineNodes.Count > 0 && dialogueData != null)
        {
            Debug.LogWarning("inlineとdataの両方が設定されています（inline優先）", this);
        }
    }
}