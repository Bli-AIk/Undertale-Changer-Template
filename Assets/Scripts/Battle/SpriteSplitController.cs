using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 内含Pool。
/// 实现精灵的碎片化效果。
/// 一行一行自上而下。
/// 启 用 本 脚 本 后 立 刻 生 效
/// </summary>
public class SpriteSplitController : MonoBehaviour
{
    private readonly Queue<GameObject> available = new();//对象池
    private Texture2D map;
    private GameObject Mask;
    public int poolCount;
    public List<Color> colorExclude;
    public Vector2 startPos;//粒子为计算出图片左上角的相对坐标
    public float speed;//粒子生成速度

    private void Awake()
    {
        map = GetComponent<SpriteRenderer>().sprite.texture;
        Mask = transform.Find("Mask").gameObject;
    }

    private void OnEnable()
    {
        startPos = new Vector2(-map.width / 2 * 0.05f, map.height / 2 * 0.05f);
        if (map.width % 2 == 0)
            startPos += new Vector2(0.025f, 0);
        if (map.height % 2 == 0)
            startPos -= new Vector2(0, 0.025f);

        Mask.transform.localScale = new Vector2(map.width, map.height);
        Mask.transform.localPosition = new Vector3(0, 0.05f * map.height);
        StartCoroutine(SummonPixel());
    }

    private IEnumerator SummonPixel()
    {
        for (int y = map.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < map.width; x++)
            {
                bool skip = false;
                Color color = map.GetPixel(x, y);
                for (int i = 0; i < colorExclude.Count; i++)
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
                else
                {
                    GameObject obj = GetFromPool();
                    obj.GetComponent<SpriteRenderer>().color = color;
                    obj.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

                    obj.transform.localPosition = startPos + new Vector2(x * 0.05f, -(map.height - y - 1) * 0.05f);
                }
            }
            Mask.transform.localPosition -= new Vector3(0, 0.05f);
            yield return new WaitForSeconds(speed);
        }
    }

    //-----对象池部分-----

    /// <summary>
    /// 初始化/填充对象池
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < poolCount; i++)
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
        available.Enqueue(gameObject);
    }

    /// <summary>
    /// 喜提对象 square)
    /// </summary>
    public GameObject GetFromPool()
    {
        if (available.Count == 0)
            FillPool();

        var square = available.Dequeue();

        square.SetActive(true);
        return square;
    }
}