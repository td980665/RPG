using System.Collections.Generic;
using UnityEngine;

public class GameFlagManager : MonoBehaviour
{
    public static GameFlagManager Instance;

    private Dictionary<string, bool> boolFlags = new Dictionary<string, bool>();
    private Dictionary<string, int> intFlags = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitFlags();
    }

    void InitFlags()
    {
        SetBool("npc_test_talked_3", false);
    }


    // ===== Bool =====
    public void SetBool(string key, bool value)
    {
        boolFlags[key] = value;
        Debug.Log($"[Flag] {key} = {value}");
    }

    public bool GetBool(string key)
    {
        if (!boolFlags.ContainsKey(key))
        {
            Debug.LogWarning($"[Flag] 未定義: {key}");

            // ▼これ追加（最重要）
            boolFlags[key] = false;

            return false;
        }

        return boolFlags[key];
    }

    // ===== Int =====
    public void SetInt(string key, int value)
    {
        intFlags[key] = value;
        Debug.Log($"[Flag] {key} = {value}");
    }

    public int GetInt(string key)
    {
        if (!intFlags.ContainsKey(key))
        {
            Debug.LogWarning($"[Flag] 未定義: {key}");
            return 0;
        }

        return intFlags[key];
    }

    public SaveData CreateSaveData()
    {
        SaveData data = new SaveData();

        foreach (var pair in boolFlags)
        {
            if (pair.Value)
            {
                data.trueFlags.Add(pair.Key);
            }
        }

        return data;
    }

    public void LoadSaveData(SaveData data)
    {
        boolFlags.Clear();

        foreach (var key in data.trueFlags)
        {
            boolFlags[key] = true;
        }
    }
    public Dictionary<string, bool> GetAllFlags()
    {
        return boolFlags;
    }
}