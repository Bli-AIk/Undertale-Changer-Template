using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 存档存储的数据
/// </summary>
public class SaveController : MonoBehaviour
{
    public static Dictionary<string, PlayerControl> usersData = new Dictionary<string, PlayerControl>();

    public static void SaveData(PlayerControl data, string dataName)
    {
        //Debug.Log("save");
        if (!Directory.Exists(Application.dataPath + "/Data"))
        {
            //Debug.Log("create");
            Directory.CreateDirectory(Application.dataPath + "/Data");
        }
        usersData[data.name] = data;
        // 转换数据
        string jsonData = JsonConvert.SerializeObject(data);

        File.WriteAllText(Application.dataPath + string.Format("/Data/{0}.json", dataName), jsonData);
    }

    // 读取用户数据到内存
    public static PlayerControl LoadData(string dataName)
    {
        //Debug.Log("load");

        SortAndRenameData();
        string path = Application.dataPath + string.Format("/Data/{0}.json", dataName);
        // 检查用户配置文件是否存在
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerControl userData = ScriptableObject.CreateInstance<PlayerControl>(); // 使用 CreateInstance 方法
            JsonConvert.PopulateObject(jsonData, userData); // 使用 PopulateObject 方法来填充数据
            usersData[dataName] = userData;
            return userData;
        }
        else
        {
            return null;
        }

    }
    public static int GetDataNum()
    {

        if (!Directory.Exists(Application.dataPath + "/Data"))
        {
            //Debug.Log("家人们谁懂啊"+ Application.dataPath + "/Data");
            return 0;
        }

        int returnNum = 0;
        for (int i = 0; i < Directory.GetFiles(Application.dataPath + "/Data").Length; i++)
        {
            string text = Directory.GetFiles(Application.dataPath + "/Data")[i];
            if (text.Substring(text.Length - 5) == ".json")
                returnNum++;
        }
        return returnNum;
    }

    public static void DeleteData(string dataName)
    {
        string path = Application.dataPath + string.Format("/Data/{0}.json", dataName);

        // 检查存档是否存在
        if (File.Exists(path))
        {
            // 从内存中移除存档数据
            if (usersData.ContainsKey(dataName))
            {
                usersData.Remove(dataName);
            }

            // 删除文件
            File.Delete(path);
        }
        else
        {
            Debug.LogWarningFormat("存档 '{0}' 不存在，无法删除。", dataName);
        }
        SortAndRenameData();
    }

    public static void SortAndRenameData()
    {
        string dataPath = Application.dataPath + "/Data";

        if (!Directory.Exists(dataPath))
        {
            Debug.LogWarning("存档目录不存在，无法进行排序和重命名。");
            return;
        }

        // 获取目录下所有的存档文件路径
        string[] files = Directory.GetFiles(dataPath, "*.json");

        // 按照文件名的数字进行排序
        Array.Sort(files, (a, b) =>
        {
            string fileNameA = Path.GetFileNameWithoutExtension(a);
            string fileNameB = Path.GetFileNameWithoutExtension(b);
            int numberA, numberB;

            if (int.TryParse(fileNameA.Substring(4), out numberA) && int.TryParse(fileNameB.Substring(4), out numberB))
            {
                return numberA.CompareTo(numberB);
            }

            return fileNameA.CompareTo(fileNameB);
        });

        // 重命名文件
        for (int i = 0; i < files.Length; i++)
        {
            string newFileName = string.Format("Data{0}.json", i);
            string newPath = Path.Combine(dataPath, newFileName);
            File.Move(files[i], newPath);
        }
    }

}

