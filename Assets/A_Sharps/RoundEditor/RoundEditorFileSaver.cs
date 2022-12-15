using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 基于对象池 在回合编辑器内处理文件(图标)
/// </summary>
public class RoundEditorFileSaver : MonoBehaviour
{
    [Header("填充对象池的对象数量")]
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

    //-----对象池部分-----

    /// <summary>
    /// 初始化/填充对象池
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
    /// 返回对象池
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        availblespriteFile.Enqueue(gameObject);
    }
    /// <summary>
    /// 喜提对象
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
