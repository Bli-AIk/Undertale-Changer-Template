using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Global.Core
{
    public class ObjectPool : MonoBehaviour
    {
        public bool isStartFillPool;
        [Header("填充对象池的对象数量")] public int count = 10;

        [FormerlySerializedAs("obj")] public GameObject poolObject;
        public Transform parent;

        // 存储对象和其对应的脚本
        private readonly Queue<(GameObject gameObject, Component component)> _availableObj = new();

        private void Start()
        {
            if (isStartFillPool)
            {
                FillPool<Transform>();
            }
        }

        /// <summary>
        /// 初始化/填充对象池
        /// </summary>
        public void FillPool<T>() where T : Component
        {
            if (!parent)
            {
                parent = transform;
            }

            for (var i = 0; i < count; i++)
            {
                var newObj = Instantiate(poolObject, parent);

                Component script = transform;
                if (typeof(T) != typeof(Transform))
                {
                    script = newObj.GetComponent<T>();
                    if (!script)
                    {
                        script = newObj.AddComponent<T>(); // 如果没有组件，自动添加
                    }
                }

                ReturnPool(newObj, script);
            }
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        public void ReturnPool<T>(GameObject inputGameObject, T script) where T : Component
        {
            if (!parent)
            {
                parent = transform;
            }

            inputGameObject.SetActive(false);
            inputGameObject.transform.SetParent(parent);

            _availableObj.Enqueue((inputGameObject, script));
        }

        /// <summary>
        /// 获取对象池中的物体或脚本
        /// </summary>
        public T GetFromPool<T>() where T : Component
        {
            if (_availableObj.Count == 0)
            {
                FillPool<T>();
            }

            var (availableObj, component) = _availableObj.Dequeue();

            availableObj.SetActive(true);

            if (typeof(T) == typeof(Transform))
            {
                return availableObj.transform as T;
            }

            return component as T;
        }
    }
}
