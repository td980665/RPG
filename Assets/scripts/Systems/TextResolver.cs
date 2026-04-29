using UnityEngine;

public static class TextResolver
{
    public static string Resolve(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = text.Replace("{player}", "勇者"); // 仮固定

        return text;
    }
}