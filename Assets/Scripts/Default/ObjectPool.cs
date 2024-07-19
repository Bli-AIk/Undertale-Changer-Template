using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Number of objects to populate the object pool")]
    public int count = 10;

    public GameObject obj;
    private Queue<GameObject> availableObj = new Queue<GameObject>();
    public Transform parent = null;

    /// <summary>
    /// Initialize/Populate Object Pool
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
    /// Returns the object pool
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
    /// Hippie Objects
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
