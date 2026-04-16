using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    public enum NodeType
    {
        Line,
        Choice,
        Branch
    }

    private Dictionary<string, DialogueNode> nodeDict;

    private Dictionary<string, int> visitCount = new Dictionary<string, int>();

    private bool isSelecting = false;

    private int safetyCounter = 0;
    private const int MAX_LOOP = 20;

    public void StartDialogue(DialogueData data, string startId)
    {
        if (data == null)
        {
            Debug.LogError("DialogueData is null");
            return;
        }

        nodeDict = data.nodes.ToDictionary(n => n.id);

        // 追加
        foreach (var n in nodeDict.Values)
        {
            ValidateNode(n);
        }

        safetyCounter = 0;
        visitCount.Clear();

        Debug.Log($"[Dialogue] Start: {startId}");

        RunNode(startId);
    }

    void RunNode(string id)
    {
        if (!visitCount.ContainsKey(id))
            visitCount[id] = 0;

        visitCount[id]++;

        if (visitCount[id] > 3)
        {
            Debug.LogError("Loop suspected: " + id);
            DialogueManager.Instance.EndDialogue();
            return;
        }

        safetyCounter++;
        Debug.Log($"[LoopCount] {safetyCounter}");

        if (safetyCounter > MAX_LOOP)
        {
            Debug.LogError("Infinite loop detected");
            DialogueManager.Instance.EndDialogue();
            return;
        }

        if (!nodeDict.ContainsKey(id))
        {
            Debug.LogError("Node not found: " + id);
            DialogueManager.Instance.EndDialogue();
            return;
        }

        var node = nodeDict[id];

        if (!CheckConditions(node.conditions))
        {
            Debug.LogWarning($"Condition not met: {id} → skip to {node.nextNodeId}");

            // 次があればスキップ
            if (!string.IsNullOrEmpty(node.nextNodeId))
            {
                Next(node.nextNodeId);
            }
            else
            {
                DialogueManager.Instance.EndDialogue();
            }
            return;
        }

        switch (node.type)
        {
            case NodeType.Line:
                RunLine(node);
                break;

            case NodeType.Choice:
                RunChoice(node);
                break;

            case NodeType.Branch:
                RunBranch(node);
                break;
        }
    }

    System.Collections.IEnumerator NextNodeNextFrame(DialogueNode node)
    {
        yield return null;
        AfterNode(node);
    }

    void AfterNode(DialogueNode node)
    {
        if (!string.IsNullOrEmpty(node.nextNodeId))
        {
            Next(node.nextNodeId);
            return;
        }

        DialogueManager.Instance.EndDialogue();
    }

    void ShowChoices(List<Choice> choices)
    {
        var dm = DialogueManager.Instance;

        // ▼ 念のため空除外（防御）
        var filtered = choices
            .Where(c => !string.IsNullOrEmpty(c.text))
            .ToList();

        // ▼ 表示するものがなければUI出さない
        if (filtered.Count == 0)
        {
            Debug.LogError("All choices filtered out (text missing)");
            dm.choicesUI.SetActive(false);
            DialogueManager.Instance.EndDialogue();
            return;
        }

        dm.choicesUI.SetActive(true);
        dm.choiceButton1.SetActive(false);
        dm.choiceButton2.SetActive(false);

        if (filtered.Count >= 1)
            SetupChoiceButton(dm.choiceButton1, filtered[0]);

        if (filtered.Count >= 2)
            SetupChoiceButton(dm.choiceButton2, filtered[1]);
    }

    void SetupChoiceButton(GameObject buttonObj, Choice choice)
    {
        if (string.IsNullOrEmpty(choice.text))
        {
            buttonObj.SetActive(false);
            return;
        }

        buttonObj.SetActive(true);

        var text = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        text.text = choice.text;

        var btn = buttonObj.GetComponent<UnityEngine.UI.Button>();
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() =>
        {
            OnChoiceSelected(choice);
        });
    }

    void OnChoiceSelected(Choice choice)
    {
        if (isSelecting) return;
        isSelecting = true;

        DialogueManager.Instance.choicesUI.SetActive(false);

        ApplyEffects(choice.effects);

        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            Next(choice.nextNodeId);
        }
        else
        {
            DialogueManager.Instance.EndDialogue();
        }

        // ❌ 消す
        // isSelecting = false;
    }

    bool CheckConditions(List<Condition> conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            Debug.Log("[Check] 条件なし → true");
            return true;
        }

        foreach (var c in conditions)
        {
            bool flag = GameFlagManager.Instance.GetBool(c.key);

            Debug.Log($"[Check] {c.key} = {flag} (need {c.value})");

            if (flag != c.value)
            {
                Debug.Log($"[Check FAIL] {c.key}");
                return false;
            }
        }

        return true;
    }

    void RunLine(DialogueNode node)
    {
        if (node.lines == null || node.lines.Length == 0)
        {
            Debug.LogError($"Line node has no text: {node.id}");
            Next(node.nextNodeId);
            return;
        }

        DialogueManager.Instance.onDialogueEnd = () =>
        {
            Next(node.nextNodeId);
        };

        var resolvedLines = node.lines
             .Select(line => TextResolver.Resolve(line))
             .ToArray();

        DialogueManager.Instance.StartDialogue(
            new[]
            {
        new DialogueEntry { lines = resolvedLines }
            }
        );
    }

    void RunChoice(DialogueNode node)
    {
        if (node.choices == null || node.choices.Count == 0)
        {
            Debug.LogError($"Choice runtime empty: {node.id}");
            DialogueManager.Instance.EndDialogue();
            return;
        }

        var validChoices = node.choices
            .Where(c => CheckConditions(c.conditions))
            .ToList();

        if (validChoices.Count == 0)
        {
            Debug.LogError($"No valid choices: {node.id}");
            DialogueManager.Instance.EndDialogue();
            return;
        }

        ShowChoices(validChoices);
    }

    void RunBranch(DialogueNode node)
    {
        if (node.branches == null || node.branches.Count == 0)
        {
            Debug.LogError($"Branch runtime empty: {node.id}");
            DialogueManager.Instance.EndDialogue();
            return;
        }

        foreach (var b in node.branches.OrderByDescending(x => x.priority))
        {
            if (CheckConditions(b.conditions))
            {
                Next(b.nextNodeId);
                return;
            }
        }

        Debug.LogError($"Branch fallback missing: {node.id}");
        DialogueManager.Instance.EndDialogue();
    }

    void Next(string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            DialogueManager.Instance.EndDialogue();
            return;
        }

        RunNode(nextId); // ← 直接呼ぶ
    }

    void ValidateNode(DialogueNode node)
    {
        // 既存チェック…

        // ▼ nextNode存在チェック
        if (!string.IsNullOrEmpty(node.nextNodeId) && !nodeDict.ContainsKey(node.nextNodeId))
        {
            Debug.LogError($"Next node not found: {node.id} -> {node.nextNodeId}");
        }

        // ▼ Choice遷移チェック
        if (node.choices != null)
        {
            foreach (var c in node.choices)
            {
                if (!string.IsNullOrEmpty(c.nextNodeId) && !nodeDict.ContainsKey(c.nextNodeId))
                {
                    Debug.LogError($"Choice next invalid: {node.id} -> {c.nextNodeId}");
                }
            }
        }

        // ▼ Branch遷移チェック
        if (node.branches != null)
        {
            foreach (var b in node.branches)
            {
                if (!string.IsNullOrEmpty(b.nextNodeId) && !nodeDict.ContainsKey(b.nextNodeId))
                {
                    Debug.LogError($"Branch next invalid: {node.id} -> {b.nextNodeId}");
                }
            }
        }
    }

    public void StartDialogue(List<DialogueNode> nodes, string startId)
    {
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogError("Node list is empty");
            return;
        }

        // ▼ ここが重要
        var data = new DialogueData();
        data.nodes = nodes;

        StartDialogue(data, startId);
    }

    void ApplyEffects(List<Effect> effects)
    {
        if (effects == null) return;

        foreach (var e in effects)
        {
            GameFlagManager.Instance.SetBool(e.key, e.value);
        }
    }
}