using System.Collections.Generic;
using UnityEngine;

namespace UCT.Global.Core
{
    public class ObjectPool : MonoBehaviour
    {
        [Header("填充对象池的对象数量")]
        public int count = 10;

        public GameObject obj;
        private Queue<GameObject> _availableObj = new Queue<GameObject>();
        public Transform parent;

        /// <summary>
        /// 初始化/填充对象池
        /// </summary>
        public virtual void FillPool()
        {
            if (parent == null)
                parent = transform;

            for (var i = 0; i < count; i++)
            {
                var newObj = Instantiate(obj, parent);
                ReturnPool(newObj);
            }
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        public virtual void ReturnPool(GameObject gameObject)
        {
            if (parent == null)
                parent = transform;

            gameObject.SetActive(false);
            gameObject.transform.SetParent(parent);
            _availableObj.Enqueue(gameObject);
        }

        /// <summary>
        /// 喜提对象
        /// </summary>
        public virtual GameObject GetFromPool()
        {
            if (_availableObj.Count == 0)
                FillPool();

            var obj = _availableObj.Dequeue();

            obj.SetActive(true);
            return obj;
        }
    }
}
