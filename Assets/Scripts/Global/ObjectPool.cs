using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("填充对象池的对象数量")]
    public int count = 10;

    public GameObject obj;
    private Queue<GameObject> availableObj = new Queue<GameObject>();
    public Transform parent = null;

    /// <summary>
    /// 初始化/填充对象池
    /// </summary>
    public void FillPool()
    {
        if (parent == null)
            parent = transform;

        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(obj, parent);
            ReturnPool(newObj);
        }
    }

    /// <summary>
    /// 返回对象池
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        if (parent == null)
            parent = transform;

        gameObject.SetActive(false);
        gameObject.transform.SetParent(parent);
        availableObj.Enqueue(gameObject);
    }

    /// <summary>
    /// 喜提对象
    /// </summary>
    public GameObject GetFromPool()
    {
        if (availableObj.Count == 0)
            FillPool();

        var obj = availableObj.Dequeue();

        obj.SetActive(true);
        return obj;
    }
}