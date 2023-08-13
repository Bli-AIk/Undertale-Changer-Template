using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [Header("Number of objects to fill the object pool")]
    public int count = 10;

    public GameObject obj;
    Queue<GameObject> availableObj = new Queue<GameObject>();
    public Transform parent = null;
    /// <summary>
    /// Initialize/Fill Object Pool
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
    /// Return Object Pool
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
    /// Congratulations on getting an object :)
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
