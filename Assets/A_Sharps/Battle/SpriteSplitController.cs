using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ں�Pool��
/// ʵ�־������Ƭ��Ч����
/// һ��һ�����϶��¡�
/// �� �� �� �� �� �� �� �� �� Ч
/// </summary>
public class SpriteSplitController : MonoBehaviour
{
    Queue<GameObject> availble = new Queue<GameObject>();//�����
    Texture2D map;
    GameObject Mask;
    public int poolCount;
    public List<Color> colorExclude;
    public Vector2 startPos;//����Ϊ�����ͼƬ���Ͻǵ��������
    public float speed;//���������ٶ�
    void Awake()
    {
        map = GetComponent<SpriteRenderer>().sprite.texture;
        Mask = transform.Find("Mask").gameObject;
    }
    void OnEnable()
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
    IEnumerator SummonPixel()
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

    //-----����ز���-----

    /// <summary>
    /// ��ʼ��/�������
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
    /// ���ض����
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        availble.Enqueue(gameObject);
    }
    /// <summary>
    /// ϲ����� square)
    /// </summary>
    public GameObject GetFromPool()
    {
        if (availble.Count == 0)
            FillPool();

        var square = availble.Dequeue();

        square.SetActive(true);
        return square;
    }


}
