using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Save stored data
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
        // Convert Data
        string jsonData = JsonConvert.SerializeObject(data);

        File.WriteAllText(Application.dataPath + string.Format("/Data/{0}.json", dataName), jsonData);
    }

    //Read user data to memory
    public static PlayerControl LoadData(string dataName)
    {
        //Debug.Log("load");

        SortAndRenameData();
        string path = Application.dataPath + string.Format("/Data/{0}.json", dataName);
        // Check if the user profile exists
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerControl userData = ScriptableObject.CreateInstance<PlayerControl>(); // Using the CreateInstance function
            JsonConvert.PopulateObject(jsonData, userData); // Using the PopulateObject function to fill in data
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
            //Debug.Log("Never gonna give you up"+ Application.dataPath + "/Data");
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
            // Removing archived data from memory
            if (usersData.ContainsKey(dataName))
            {
                usersData.Remove(dataName);
            }

            // delete file
            File.Delete(path);
        }
        else
        {
            Debug.LogWarningFormat("The archive '{0}' does not exist and cannot be deleted.", dataName);
        }
        SortAndRenameData();
    }

    public static void SortAndRenameData()
    {
        string dataPath = Application.dataPath + "/Data";

        if (!Directory.Exists(dataPath))
        {
            Debug.LogWarning("The archive directory does not exist and cannot be sorted or renamed.");
            return;
        }

        // Obtain all archive file paths under the directory
        string[] files = Directory.GetFiles(dataPath, "*.json");

        // Sort by number of file names
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

        // rename file
        for (int i = 0; i < files.Length; i++)
        {
            string newFileName = string.Format("Data{0}.json", i);
            string newPath = Path.Combine(dataPath, newFileName);
            File.Move(files[i], newPath);
        }
    }

}

