using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    private string searchText = "";
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        DialogueData data = (DialogueData)target;

        if (data.nodes == null)
            return;

        GUILayout.Label("Search", EditorStyles.boldLabel);

        searchText =
            EditorGUILayout.TextField(searchText);

        if (GUILayout.Button("Sort By ID"))
        {
            Undo.RecordObject(data, "Sort Dialogue Nodes");

            data.nodes =
                data.nodes
                .OrderBy(n => n.id)
                .ToList();

            EditorUtility.SetDirty(data);
        }

        GUILayout.Space(10);

        GUILayout.Label("Node Preview", EditorStyles.boldLabel);

        foreach (var node in data.nodes)
        {
            if (node == null) continue;

            if (!foldouts.ContainsKey(node.id))
                foldouts[node.id] = false;

            // Search filter
            if (!string.IsNullOrEmpty(searchText))
            {
                bool match = false;
                if (!string.IsNullOrEmpty(node.id) && node.id.ToLower().Contains(searchText.ToLower()))
                    match = true;

                if (node.lines != null)
                {
                    foreach (var line in node.lines)
                    {
                        if (!string.IsNullOrEmpty(line) && line.ToLower().Contains(searchText.ToLower()))
                        {
                            match = true;
                            break;
                        }
                    }
                }

                if (node.choices != null)
                {
                    foreach (var c in node.choices)
                    {
                        if (!string.IsNullOrEmpty(c.text) && c.text.ToLower().Contains(searchText.ToLower()))
                        {
                            match = true;
                            break;
                        }
                    }
                }

                if (!match) continue;
            }

            foldouts[node.id] = EditorGUILayout.Foldout(foldouts[node.id], node.id, true);
            if (!foldouts[node.id]) continue;

            GUILayout.BeginVertical("box");

            GUILayout.Label($"ID: {node.id}");

            int duplicateCount = data.nodes.Count(n => n != null && n.id == node.id);
            if (duplicateCount > 1) DrawError("Duplicate ID");

            if (node.id != "start" && !IsReferenced(data, node.id)) DrawWarning("Unused node");

            if (!string.IsNullOrEmpty(node.nextNodeId))
            {
                bool exists = data.nodes.Exists(n => n != null && n.id == node.nextNodeId);
                if (!exists) DrawError("Missing nextNode: " + node.nextNodeId);
            }

            if (node.lines != null)
            {
                foreach (var line in node.lines)
                    GUILayout.Label(line);
            }

            if (!string.IsNullOrEmpty(node.nextNodeId))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Next -> " + node.nextNodeId);
                if (GUILayout.Button("Go", GUILayout.Width(40))) JumpToNode(node.nextNodeId);
                GUILayout.EndHorizontal();
            }

            if (node.choices != null && node.choices.Count > 0)
            {
                GUILayout.Label("Choices:");
                foreach (var c in node.choices)
                {
                    bool exists = data.nodes.Exists(n => n != null && n.id == c.nextNodeId);
                    if (!exists) DrawError("Missing choice node: " + c.nextNodeId);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"- {c.text} -> {c.nextNodeId}");
                    if (GUILayout.Button("Go", GUILayout.Width(40))) JumpToNode(c.nextNodeId);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    bool IsReferenced(DialogueData data, string nodeId)
    {
        foreach (var n in data.nodes)
        {
            if (n == null) continue;

            if (n.nextNodeId == nodeId) return true;

            if (n.choices != null)
            {
                foreach (var c in n.choices)
                {
                    if (c.nextNodeId == nodeId) return true;
                }
            }

            if (n.branches != null)
            {
                foreach (var b in n.branches)
                {
                    if (b.nextNodeId == nodeId) return true;
                }
            }
        }

        return false;
    }

    void DrawError(string message)
    {
        var prev = GUI.color;
        GUI.color = Color.red;
        GUILayout.Label(message);
        GUI.color = prev;
    }

    void DrawWarning(string message)
    {
        var prev = GUI.color;
        GUI.color = Color.yellow;
        GUILayout.Label(message);
        GUI.color = prev;
    }

    void JumpToNode(string nodeId)
    {
        searchText = nodeId;

        var keys = foldouts.Keys.ToList();
        foreach (var key in keys)
            foldouts[key] = key == nodeId;

        Repaint();
        GUI.FocusControl(null);
    }
}
