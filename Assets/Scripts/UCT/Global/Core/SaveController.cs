using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UCT.Control;
using UnityEngine;

namespace UCT.Global.Core
{
    /// <summary>
    ///     存档存储的数据
    /// </summary>
    public class SaveController : MonoBehaviour
    {
        private static readonly Dictionary<string, PlayerControl> UsersData = new();

        public static void SaveData(PlayerControl data, string dataName)
        {
            if (!Directory.Exists($"{Application.dataPath}/Data"))
            {
                Directory.CreateDirectory($"{Application.dataPath}/Data");
            }

            UsersData[data.name] = data;
            
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var jsonData = JsonConvert.SerializeObject(data, settings);

            File.WriteAllText(Application.dataPath + $"/Data/{dataName}.json", jsonData);
        }

        public static PlayerControl LoadData(string dataName)
        {
            SortAndRenameData();
            var path = Application.dataPath + $"/Data/{dataName}.json";
            if (!File.Exists(path))
            {
                return null;
            }

            var jsonData = File.ReadAllText(path);
            var userData = ScriptableObject.CreateInstance<PlayerControl>(); 
            JsonConvert.PopulateObject(jsonData, userData); 
            UsersData[dataName] = userData;
            return userData;

        }

        public static int GetDataNumber()
        {
            if (!Directory.Exists($"{Application.dataPath}/Data"))
            {
                return 0;
            }

            var returnNumber = 0;
            for (var i = 0; i < Directory.GetFiles($"{Application.dataPath}/Data").Length; i++)
            {
                var text = Directory.GetFiles($"{Application.dataPath}/Data")[i];
                if (text[^5..] == ".json")
                {
                    returnNumber++;
                }
            }

            return returnNumber;
        }

        public static void DeleteData(string dataName)
        {
            var path = Application.dataPath + $"/Data/{dataName}.json";

            if (File.Exists(path))
            {
                if (UsersData.ContainsKey(dataName))
                {
                    UsersData.Remove(dataName);
                }

                File.Delete(path);
            }
            else
            {
                Other.Debug.Log($"存档{dataName}不存在，无法删除。");
            }

            SortAndRenameData();
        }

        private static void SortAndRenameData()
        {
            var dataPath = Application.dataPath + "/Data";

            if (!Directory.Exists(dataPath))
            {
                Other.Debug.Log("存档目录不存在，无法进行排序和重命名。");
                return;
            }

            var files = Directory.GetFiles(dataPath, "*.json");

            Array.Sort(files, (a, b) =>
            {
                var fileNameA = Path.GetFileNameWithoutExtension(a);
                var fileNameB = Path.GetFileNameWithoutExtension(b);

                if (int.TryParse(fileNameA[4..], out var numberA) && int.TryParse(fileNameB[4..], out var numberB))
                {
                    return numberA.CompareTo(numberB);
                }

                return string.Compare(fileNameA, fileNameB, StringComparison.Ordinal);
            });

            for (var i = 0; i < files.Length; i++)
            {
                var newFileName = $"Data{i}.json";
                var newPath = Path.Combine(dataPath, newFileName);
                File.Move(files[i], newPath);
            }
        }
    }
}