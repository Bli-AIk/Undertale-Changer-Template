using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Log;
/// <summary>
/// Archived stored data
/// </summary>
public class SaveController : MonoBehaviour
{
    public static Dictionary<string, PlayerControl> usersData = new Dictionary<string, PlayerControl>();

    public static void SaveData(PlayerControl data, string dataName)
    {
        //DebugLogger.Log("save");
        if (!Directory.Exists(Application.dataPath + "/Data"))
        {
            //DebugLogger.Log("create");
            Directory.CreateDirectory(Application.dataPath + "/Data");
        }
        usersData[data.name] = data;
        // Convert data
        string jsonData = JsonConvert.SerializeObject(data);

        File.WriteAllText(Application.dataPath + string.Format("/Data/{0}.json", dataName), jsonData);
    }

    // read user data into memory
    public static PlayerControl LoadData(string dataName)
    {
        //DebugLogger.Log("load");

        SortAndRenameData();
        string path = Application.dataPath + string.Format("/Data/{0}.json", dataName);
        // Check if the user profile exists.
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerControl userData = ScriptableObject.CreateInstance<PlayerControl>();
            // Using the CreateInstance method
            JsonConvert.PopulateObject(jsonData, userData);
            // Use the PopulateObject method to populate the data.
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
            //DebugLogger.Log("Families who get it" + Application.dataPath + "/Data");
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

        // Check if the archive exists
        if (File.Exists(path))
        {
            // remove archive data from memory
            if (usersData.ContainsKey(dataName))
            {
                usersData.Remove(dataName);
            }

            // Delete the file
            File.Delete(path);
        }
        else
        {
            DebugLogger.Log("存档"+ dataName+"不存在，无法删除。",DebugLogger.Type.war);
        }
        SortAndRenameData();
    }

    public static void SortAndRenameData()
    {
        string dataPath = Application.dataPath + "/Data";

        if (!Directory.Exists(dataPath))
        {
            DebugLogger.Log("存档目录不存在，无法进行排序和重命名。", DebugLogger.Type.war);
            return;
        }

        // Get the paths of all archive files in the directory
        string[] files = Directory.GetFiles(dataPath, "*.json");

        // Sort filenames numerically
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

        // Rename the file
        for (int i = 0; i < files.Length; i++)
        {
            string newFileName = string.Format("Data{0}.json", i);
            string newPath = Path.Combine(dataPath, newFileName);
            File.Move(files[i], newPath);
        }
    }
}
