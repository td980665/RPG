using System.Text;
using TMPro;
using UnityEngine;

public class FlagDebugViewer : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        if (text == null) return;

        var flags = GameFlagManager.Instance.GetAllFlags();

        StringBuilder sb = new StringBuilder();

        foreach (var pair in flags)
        {
            sb.AppendLine(pair.Key + " = " + pair.Value);
        }

        text.text = sb.ToString();
    }
}