using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveSystem
{
    private const string SAVE_FILE_NAME = "/gamesave.json";

    public static void Save(SaveData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        string path = Application.persistentDataPath + SAVE_FILE_NAME;

        File.WriteAllText(path, json);
        Debug.Log($"Данные сохранены в {path}");
    }

    public static SaveData Load()
    {
        string path = Application.persistentDataPath + SAVE_FILE_NAME;
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);

        try
        {
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

            // UPD
            if (data == null) return null;
            if(data.version != new SaveData().version)
            {
                data.version = new SaveData().version;
                Save(data);
            }

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки сохранения: {e}");
            return new SaveData();
        }

    }

    public static void Delete()
    {
        string path = Application.persistentDataPath + SAVE_FILE_NAME;

        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
