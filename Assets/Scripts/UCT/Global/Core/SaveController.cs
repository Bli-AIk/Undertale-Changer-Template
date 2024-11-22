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
        public static Dictionary<string, PlayerControl> UsersData = new();

        public static void SaveData(PlayerControl data, string dataName)
        {
            //Debug.Log("save");
            if (!Directory.Exists(Application.dataPath + "/Data"))
                //Debug.Log("create");
                Directory.CreateDirectory(Application.dataPath + "/Data");
            UsersData[data.name] = data;
            // 转换数据
            var jsonData = JsonConvert.SerializeObject(data);

            File.WriteAllText(Application.dataPath + string.Format("/Data/{0}.json", dataName), jsonData);
        }

        // 读取用户数据到内存
        public static PlayerControl LoadData(string dataName)
        {
            //Debug.Log("load");

            SortAndRenameData();
            var path = Application.dataPath + string.Format("/Data/{0}.json", dataName);
            // 检查用户配置文件是否存在
            if (File.Exists(path))
            {
                var jsonData = File.ReadAllText(path);
                var userData = ScriptableObject.CreateInstance<PlayerControl>(); // 使用 CreateInstance 方法
                JsonConvert.PopulateObject(jsonData, userData); // 使用 PopulateObject 方法来填充数据
                UsersData[dataName] = userData;
                return userData;
            }

            return null;
        }

        public static int GetDataNumber()
        {
            if (!Directory.Exists(Application.dataPath + "/Data")) return 0;

            var returnNumber = 0;
            for (var i = 0; i < Directory.GetFiles(Application.dataPath + "/Data").Length; i++)
            {
                var text = Directory.GetFiles(Application.dataPath + "/Data")[i];
                if (text[^5..] == ".json")
                    returnNumber++;
            }

            return returnNumber;
        }

        public static void DeleteData(string dataName)
        {
            var path = Application.dataPath + string.Format("/Data/{0}.json", dataName);

            // 检查存档是否存在
            if (File.Exists(path))
            {
                // 从内存中移除存档数据
                if (UsersData.ContainsKey(dataName)) UsersData.Remove(dataName);

                // 删除文件
                File.Delete(path);
            }
            else
            {
                Other.Debug.Log($"存档{dataName}不存在，无法删除。");
            }

            SortAndRenameData();
        }

        public static void SortAndRenameData()
        {
            var dataPath = Application.dataPath + "/Data";

            if (!Directory.Exists(dataPath))
            {
                Other.Debug.Log("存档目录不存在，无法进行排序和重命名。");
                return;
            }

            // 获取目录下所有的存档文件路径
            var files = Directory.GetFiles(dataPath, "*.json");

            // 按照文件名的数字进行排序
            Array.Sort(files, (a, b) =>
            {
                var fileNameA = Path.GetFileNameWithoutExtension(a);
                var fileNameB = Path.GetFileNameWithoutExtension(b);
                int numberA, numberB;

                if (int.TryParse(fileNameA[4..], out numberA) && int.TryParse(fileNameB[4..], out numberB))
                    return numberA.CompareTo(numberB);

                return fileNameA.CompareTo(fileNameB);
            });

            // 重命名文件
            for (var i = 0; i < files.Length; i++)
            {
                var newFileName = string.Format("Data{0}.json", i);
                var newPath = Path.Combine(dataPath, newFileName);
                File.Move(files[i], newPath);
            }
        }
    }
}