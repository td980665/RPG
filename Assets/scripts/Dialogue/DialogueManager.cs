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

        // 選択肢表示中は進めない
        if (
            dialogueUI.activeSelf &&
            Input.GetKeyDown(KeyCode.Space) &&
            !choicesUI.activeSelf
        )
        {
            NextLine();
        }
    }

    public void ShowLines(string[] newLines)
    {
        dialogueUI.SetActive(true);
        isDialogueActive = true;

        lines = newLines;
        index = 0;

        if (lines != null && lines.Length > 0)
        {
            dialogueText.text = lines[0];
        }
    }

    void NextLine()
    {
        index++;

        if (lines == null || index >= lines.Length)
        {
            Debug.Log("Lines finished");

            onDialogueEnd?.Invoke();
            onDialogueEnd = null;

            return;
        }

        dialogueText.text = lines[index];
    }

    public void ShowChoices(
        List<Choice> choices,
        System.Action<Choice> onSelected
    )
    {
        choicesUI.SetActive(true);

        choiceButton1.SetActive(false);
        choiceButton2.SetActive(false);

        if (choices.Count >= 1)
        {
            SetupChoiceButton(
                choiceButton1,
                choices[0],
                onSelected
            );
        }

        if (choices.Count >= 2)
        {
            SetupChoiceButton(
                choiceButton2,
                choices[1],
                onSelected
            );
        }
    }

    void SetupChoiceButton(
        GameObject buttonObj,
        Choice choice,
        System.Action<Choice> onSelected
    )
    {
        buttonObj.SetActive(true);

        var text =
            buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        text.text = choice.text;

        var btn = buttonObj.GetComponent<Button>();

        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() =>
        {
            choicesUI.SetActive(false);

            onSelected?.Invoke(choice);
        });
    }

    public void EndDialogue()
    {
        Debug.Log("EndDialogue called");

        dialogueUI.SetActive(false);
        choicesUI.SetActive(false);

        isDialogueActive = false;

        lines = null;
        index = 0;

        onDialogueEnd = null;
    }
}