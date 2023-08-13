using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// �浵�洢������
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
        // ת������
        string jsonData = JsonConvert.SerializeObject(data);

        File.WriteAllText(Application.dataPath + string.Format("/Data/{0}.json", dataName), jsonData);
    }

    // ��ȡ�û����ݵ��ڴ�
    public static PlayerControl LoadData(string dataName)
    {
        //Debug.Log("load");

        SortAndRenameData();
        string path = Application.dataPath + string.Format("/Data/{0}.json", dataName);
        // ����û������ļ��Ƿ����
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerControl userData = ScriptableObject.CreateInstance<PlayerControl>(); // ʹ�� CreateInstance ����
            JsonConvert.PopulateObject(jsonData, userData); // ʹ�� PopulateObject �������������
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
            //Debug.Log("������˭����"+ Application.dataPath + "/Data");
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

        // ���浵�Ƿ����
        if (File.Exists(path))
        {
            // ���ڴ����Ƴ��浵����
            if (usersData.ContainsKey(dataName))
            {
                usersData.Remove(dataName);
            }

            // ɾ���ļ�
            File.Delete(path);
        }
        else
        {
            Debug.LogWarningFormat("�浵 '{0}' �����ڣ��޷�ɾ����", dataName);
        }
        SortAndRenameData();
    }

    public static void SortAndRenameData()
    {
        string dataPath = Application.dataPath + "/Data";

        if (!Directory.Exists(dataPath))
        {
            Debug.LogWarning("�浵Ŀ¼�����ڣ��޷������������������");
            return;
        }

        // ��ȡĿ¼�����еĴ浵�ļ�·��
        string[] files = Directory.GetFiles(dataPath, "*.json");

        // �����ļ��������ֽ�������
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

        // �������ļ�
        for (int i = 0; i < files.Length; i++)
        {
            string newFileName = string.Format("Data{0}.json", i);
            string newPath = Path.Combine(dataPath, newFileName);
            File.Move(files[i], newPath);
        }
    }

}

