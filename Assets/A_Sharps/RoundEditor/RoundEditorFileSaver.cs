using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ڶ���� �ڻغϱ༭���ڴ����ļ�(ͼ��)
/// </summary>
public class RoundEditorFileSaver : MonoBehaviour
{
    [Header("������صĶ�������")]
    public int count;
    Queue<GameObject> availblespriteFile = new Queue<GameObject>();

    GameObject spriteFile;
    public List<GameObject> files = new List<GameObject>();
    private void Awake()
    {
        spriteFile = Resources.Load<GameObject>("RoundEditor/File");

        spriteFile.SetActive(false);
        FillPool();
    }

    //-----����ز���-----

    /// <summary>
    /// ��ʼ��/�������
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(spriteFile, transform);
            ReturnPool(newObj);
        }
    }
    /// <summary>
    /// ���ض����
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        availblespriteFile.Enqueue(gameObject);
    }
    /// <summary>
    /// ϲ�����
    /// </summary>
    public GameObject GetFromPool()
    {
        if (availblespriteFile.Count == 0)
            FillPool();

        var spriteFile = availblespriteFile.Dequeue();

        spriteFile.SetActive(true);
        return spriteFile;
    }

}
