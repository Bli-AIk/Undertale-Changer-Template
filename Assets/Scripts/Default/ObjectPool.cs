using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [Header("填充对象池的对象数量")]
    public int count = 10;

    public GameObject obj;
    Queue<GameObject> availableObj = new Queue<GameObject>();
    /// <summary>
    /// 初始化/填充对象池
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(obj, transform);
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
