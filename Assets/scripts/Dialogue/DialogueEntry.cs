using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    [Header("デバッグ用名前")]
    public string debugName;

    [Header("基本設定")]
    public bool isChoice;
    public int groupId;

    [Header("表示テキスト")]
    [TextArea]
    public string[] lines;

    [Header("選択肢テキスト")]
    public string choiceText;

    [Header("フラグ条件（これが必要）")]
    public string[] requiredFlags;

    [Header("フラグ設定（この会話で変化）")]
    public string flagKey;
    public bool flagValue;

    [Header("次の遷移（どちらかだけ使う）")]
    public string[] nextLines;
    public DialogueData nextDialogue;
}