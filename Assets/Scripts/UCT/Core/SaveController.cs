using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UCT.Control;
using UCT.EventSystem;
using UnityEngine;

namespace UCT.Core
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

            File.WriteAllText($"{Application.dataPath}/Data/{dataName}.json", jsonData);
            SaveFactTablesToJson($"{dataName}Table");
        }

        public static PlayerControl LoadData(string dataName)
        {
            SortAndRenameData(@"^Data(\d+)$", "Data{0}.json");
            SortAndRenameData(@"^Data(\d+)Table$", "Data{0}Table.json");
            var path = $"{Application.dataPath}/Data/{dataName}.json";
            if (!File.Exists(path))
            {
                return null;
            }

            var jsonData = File.ReadAllText(path);
            var userData = ScriptableObject.CreateInstance<PlayerControl>();
            JsonConvert.PopulateObject(jsonData, userData);
            UsersData[dataName] = userData;
            LoadFactTablesFromJson($"{dataName}Table");
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
            var path = $"{Application.dataPath}/Data/{dataName}.json";

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
                Debug.Log($"存档{dataName}不存在，无法删除。");
            }
            SortAndRenameData(@"^Data(\d+)$", "Data{0}.json");
            SortAndRenameData(@"^Data(\d+)Table$", "Data{0}Table.json");
        }

        private static void SortAndRenameData(string regexPattern, string renameFormat)
        {
            var dataPath = Application.dataPath + "/Data";

            if (!Directory.Exists(dataPath))
            {
                Debug.Log("存档目录不存在，无法进行排序和重命名。");
                return;
            }

            var regex = new Regex(regexPattern);
            var files = Directory.GetFiles(dataPath, "*.json")
                .Where(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    return regex.IsMatch(fileName);
                })
                .ToArray();

            Array.Sort(files, (a, b) =>
            {
                var fileNameA = Path.GetFileNameWithoutExtension(a);
                var fileNameB = Path.GetFileNameWithoutExtension(b);

                var matchA = regex.Match(fileNameA);
                var matchB = regex.Match(fileNameB);

                if (matchA.Success && matchB.Success && matchA.Groups.Count > 1 && matchB.Groups.Count > 1 &&
                    int.TryParse(matchA.Groups[1].Value, out var numberA) &&
                    int.TryParse(matchB.Groups[1].Value, out var numberB))
                {
                    return numberA.CompareTo(numberB);
                }
                return string.Compare(fileNameA, fileNameB, StringComparison.Ordinal);
            });

            for (var i = 0; i < files.Length; i++)
            {
                var newFileName = string.Format(renameFormat, i);
                var newPath = Path.Combine(dataPath, newFileName);
                File.Move(files[i], newPath);
            }
        }


        private static void SaveFactTablesToJson(string dataName)
        {
            var jsonDir = Path.Combine(Application.dataPath, "Data");
            if (!Directory.Exists(jsonDir))
            {
                Directory.CreateDirectory(jsonDir);
            }

            var path = Path.Combine(jsonDir, $"{dataName}.json");

            var tablesRoot = Path.Combine(Application.dataPath, "Resources", "Tables");
            if (!Directory.Exists(tablesRoot))
            {
                Debug.LogError($"资源目录不存在：{tablesRoot}");
                return;
            }

            var dict = new Dictionary<string, List<FactEntry>>();

            var assetFiles = Directory.GetFiles(tablesRoot, "*.asset", SearchOption.AllDirectories);
            foreach (var filePath in assetFiles)
            {
                var relativePath =
                    filePath.Replace(Path.Combine(Application.dataPath, "Resources") + Path.DirectorySeparatorChar, "");
                relativePath = Path.ChangeExtension(relativePath, null);
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                var scriptableObject = Resources.Load<ScriptableObject>(relativePath);
                if (scriptableObject is FactTable factTable)
                {
                    dict[relativePath] = factTable.facts;
                }
            }

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private static void LoadFactTablesFromJson(string dataName)
        {
            var path = Path.Combine(Application.dataPath, "Data", $"{dataName}.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"JSON 文件不存在：{path}");
                return;
            }

            var json = File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, List<FactEntry>>>(json);
            if (dict == null)
            {
                Debug.LogError("反序列化 JSON 数据失败！");
                return;
            }

            foreach (var (resourcePath, value) in dict)
            {
                var factTable = Resources.Load<FactTable>(resourcePath);
                if (factTable)
                {
                    factTable.facts = value;
                }
                else
                {
                    Debug.LogWarning($"加载 FactTable 失败：{resourcePath}");
                }
            }

        }
    }
}