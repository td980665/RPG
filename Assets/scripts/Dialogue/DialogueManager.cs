using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;

    public GameObject choicesUI;
    public GameObject choiceButton1;
    public GameObject choiceButton2;

    public bool isDialogueActive;

    private string[] lines;
    private int index;

    public System.Action onDialogueEnd;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // 通常会話の進行
        if (dialogueUI.activeSelf && Input.GetKeyDown(KeyCode.Space) && !choicesUI.activeSelf)
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueEntry[] entries)
    {
        choicesUI.SetActive(false);

        if (dialogueText == null)
        {
            Debug.LogError("dialogueText is null");
            return;
        }

        if (entries == null || entries.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueUI.SetActive(true);
        isDialogueActive = true;

        // 最初の通常会話エントリを取得
        DialogueEntry start = null;
        foreach (var e in entries)
        {
            if (!e.isChoice)
            {
                start = e;
                break;
            }
        }
        
        lines = start?.lines ?? new string[] { "(no text)" };

        index = 0;
        dialogueText.text = lines.Length > 0 ? lines[index] : string.Empty;

        // 選択肢収集
        List<DialogueEntry> choiceList = new List<DialogueEntry>();
        foreach (var e in entries)
        {
            if (e.isChoice && e.groupId == start?.groupId)
                choiceList.Add(e);
        }

        if (choiceList.Count > 0)
        {
            ShowChoices(choiceList.ToArray());
        }
        else if (lines.Length == 0)
        {
            // 会話も選択肢もない場合は即終了
            EndDialogue();
        }
    }

    void NextLine()
    {
        index++;
        if (lines == null || index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines[index];
    }

    public void ShowChoices(DialogueEntry[] choices)
    {
        Debug.Log("ShowChoices: " + choices.Length);

        var dm = DialogueManager.Instance;
        dm.choicesUI.transform.SetAsLastSibling();

        choicesUI.SetActive(true);

        choiceButton1.SetActive(false);
        choiceButton2.SetActive(false);

        if (choices.Length >= 1)
        {
            SetupChoiceButton(choiceButton1, choices[0]);
        }
        if (choices.Length >= 2)
        {
            SetupChoiceButton(choiceButton2, choices[1]);
        }
    }

    private void SetupChoiceButton(GameObject buttonObj, DialogueEntry entry)
    {
        buttonObj.SetActive(true);
        var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        text.text = entry.choiceText;

        var btn = buttonObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnChoiceSelected(entry));
    }

    private void OnChoiceSelected(DialogueEntry entry)
    {
        choicesUI.SetActive(false);

        if (!string.IsNullOrEmpty(entry.flagKey))
        {
            GameFlagManager.Instance.SetBool(entry.flagKey, entry.flagValue);
        }

        if (!string.IsNullOrEmpty(entry.nextNodeId))
        {
            var runner = FindFirstObjectByType<DialogueRunner>();
            runner.Next(entry.nextNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialogueUI.SetActive(false);
        choicesUI.SetActive(false);

        isDialogueActive = false; // ★これがないと2回目壊れる

        onDialogueEnd?.Invoke();
        onDialogueEnd = null;
    }
}