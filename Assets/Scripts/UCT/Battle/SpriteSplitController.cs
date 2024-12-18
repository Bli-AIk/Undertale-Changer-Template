using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 内含Pool。
    /// 实现精灵的碎片化效果。
    /// 一行一行自上而下。
    /// 启 用 本 脚 本 后 立 刻 生 效
    /// </summary>
    public class SpriteSplitController : MonoBehaviour
    {
        private readonly Queue<GameObject> _available = new();//对象池
        private Texture2D _map;
        private GameObject _mask;
        public int poolCount;
        public List<Color> colorExclude;
        public Vector2 startPos;//粒子为计算出图片左上角的相对坐标
        public float speed;//粒子生成速度

        private void Awake()
        {
            _map = GetComponent<SpriteRenderer>().sprite.texture;
            _mask = transform.Find("Mask").gameObject;
        }

        private void OnEnable()
        {
            startPos = new Vector2(-_map.width / 2 * 0.05f, _map.height / 2 * 0.05f);
            if (_map.width % 2 == 0)
                startPos += new Vector2(0.025f, 0);
            if (_map.height % 2 == 0)
                startPos -= new Vector2(0, 0.025f);

            _mask.transform.localScale = new Vector2(_map.width, _map.height);
            _mask.transform.localPosition = new Vector3(0, 0.05f * _map.height);
            StartCoroutine(SummonPixel());
        }

        private IEnumerator SummonPixel()
        {
            for (var y = _map.height - 1; y >= 0; y--)
            {
                for (var x = 0; x < _map.width; x++)
                {
                    var skip = false;
                    var color = _map.GetPixel(x, y);
                    for (var i = 0; i < colorExclude.Count; i++)
                    {
                        if (color == colorExclude[i])
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip)
                    {
                        continue;
                    }

                    var obj = GetFromPool();
                    obj.GetComponent<SpriteRenderer>().color = color;
                    obj.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

                    obj.transform.localPosition = startPos + new Vector2(x * 0.05f, -(_map.height - y - 1) * 0.05f);
                }
                _mask.transform.localPosition -= new Vector3(0, 0.05f);
                yield return new WaitForSeconds(speed);
            }
        }

        //-----对象池部分-----

        /// <summary>
        /// 初始化/填充对象池
        /// </summary>
        public void FillPool()
        {
            for (var i = 0; i < poolCount; i++)
            {
                var newObj = Instantiate(Resources.Load<GameObject>("Template/Square Template"), transform);
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
            _available.Enqueue(gameObject);
        }

        /// <summary>
        /// 喜提对象 square)
        /// </summary>
        public GameObject GetFromPool()
        {
            if (_available.Count == 0)
                FillPool();

            var square = _available.Dequeue();

            square.SetActive(true);
            return square;
        }
    }
}