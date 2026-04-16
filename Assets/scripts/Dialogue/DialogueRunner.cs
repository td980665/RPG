using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    private Dictionary<string, DialogueNode> nodeDict;

    private int safetyCounter = 0;
    private const int MAX_LOOP = 20;

    public void StartDialogue(DialogueData data, string startId)
    {
        Debug.Log($"[Dialogue] Start: {startId}");

        if (data == null)
        {
            Debug.LogError("DialogueData is null");
            return;
        }

        nodeDict = data.nodes.ToDictionary(n => n.id);

        safetyCounter = 0;
        RunNode(startId);
    }

    public void StartDialogue(List<DialogueNode> nodes, string startId)
    {
        nodeDict = nodes.ToDictionary(n => n.id);
        safetyCounter = 0;
        RunNode(startId);
    }

    void RunNode(string id)
    {
        Debug.Log($"[LoopCount] {safetyCounter}");
        safetyCounter++;

        if (safetyCounter > MAX_LOOP)
        {
            Debug.LogError("Infinite loop detected");

            DialogueManager.Instance.onDialogueEnd = null;
            DialogueManager.Instance.EndDialogue();

            enabled = false;
            return;
        }

        Debug.Log($"[Dialogue] Enter Node: {id}");

        if (!nodeDict.ContainsKey(id))
        {
            Debug.LogError("Node not found: " + id);
            DialogueManager.Instance.EndDialogue();
            return;
        }

        var node = nodeDict[id];

        if (!CheckConditions(node.conditions))
        {
            Debug.Log("Condition not met: " + id);
            DialogueManager.Instance.EndDialogue();
            return;
        }

        // ▼ テキストがある場合
        if (node.lines != null && node.lines.Length > 0)
        {
            DialogueManager.Instance.onDialogueEnd = null;
            DialogueManager.Instance.onDialogueEnd = () =>
            {
                StartCoroutine(NextNodeNextFrame(node));
            };

            DialogueManager.Instance.StartDialogue(
                new DialogueEntry[]
                {
                    new DialogueEntry
                    {
                        lines = node.lines
                    }
                }
            );

            ApplyEffects(node.effects);
        }
        else
        {
            Debug.Log("[Skip UI] " + id);

            ApplyEffects(node.effects);

            StartCoroutine(NextNodeNextFrame(node));
        }
    }

    System.Collections.IEnumerator RunNodeNextFrame(string id)
    {
        yield return null;
        RunNode(id);
    }

    System.Collections.IEnumerator NextNodeNextFrame(DialogueNode node)
    {
        yield return null;
        AfterNode(node);
    }

    void AfterNode(DialogueNode node)
    {
        if (node.choices != null && node.choices.Count > 0)
        {
            var validChoices = node.choices
                .Where(c => CheckConditions(c.conditions))
                .ToList();

            if (validChoices.Count == 0)
            {
                Debug.LogWarning($"No valid choices at node: {node.id}");
                DialogueManager.Instance.EndDialogue();
                return;
            }

            // ▼ 分岐専用ノード（全てtext空）
            if (validChoices.All(c => string.IsNullOrEmpty(c.text)))
            {
                var next = validChoices[0];

                ApplyEffects(next.effects);

                StartCoroutine(RunNodeNextFrame(next.nextNodeId));
                return;
            }

            // ▼ UIありノード
            ShowChoices(validChoices);
            return;
        }

        if (!string.IsNullOrEmpty(node.nextNodeId))
        {
            StartCoroutine(RunNodeNextFrame(node.nextNodeId));
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
            dm.choicesUI.SetActive(false);
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
        Debug.Log($"[Dialogue] Choice Selected: {choice.text}");

        DialogueManager.Instance.choicesUI.SetActive(false);

        ApplyEffects(choice.effects);

        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            StartCoroutine(RunNodeNextFrame(choice.nextNodeId));
        }
        else
        {
            DialogueManager.Instance.EndDialogue();
        }
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

    void ApplyEffects(List<Effect> effects)
    {
        if (effects == null) return;

        foreach (var e in effects)
        {
            GameFlagManager.Instance.SetBool(e.key, e.value);
        }
    }
}