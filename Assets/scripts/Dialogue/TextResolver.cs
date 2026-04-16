using UnityEngine;

public static class TextResolver
{
    public static string Resolve(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = text.Replace("{playerName}", PlayerData.Name);

        // ▼ ここに入れる
        if (text.Contains("{"))
        {
            Debug.LogWarning("未解決変数あり: " + text);
        }

        return text;
    }
}