using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    string savePath;

    void Awake()
    {
        Instance = this;

        savePath = Path.Combine(
            Application.persistentDataPath,
            "save.json"
        );
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load();
        }
    }

    public void Save()
    {
        SaveData data =
            GameFlagManager.Instance.CreateSaveData();

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("Saved: " + savePath);
    }

    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("Save file not found");
            return;
        }

        string json = File.ReadAllText(savePath);

        SaveData data =
            JsonUtility.FromJson<SaveData>(json);

        GameFlagManager.Instance.LoadSaveData(data);

        Debug.Log("Loaded");
    }
}