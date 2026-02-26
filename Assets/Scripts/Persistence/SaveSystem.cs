using System.IO;
using UnityEngine;

namespace SurvivorSeries.Persistence
{
    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath))
                return new SaveData();

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
