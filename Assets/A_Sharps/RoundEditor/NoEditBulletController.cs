using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ڶ���� ���ƻغϱ༭������ײ��ʾ
/// </summary>
public class NoEditBulletController : MonoBehaviour
{
    public List<Sprite> sprites;
    SpriteRenderer spriteRenderer;
    [Header("�Ƿ�ѡ����spr")]
    public bool isSelectSprite;
    public float spinSpeed, randomClockMax;
    float randomClock, euler;
    int random, dir;

    //------------------�����

    [Header("������صĶ�������")]
    public int count;
    GameObject box;
    Queue<GameObject> availbleBox = new Queue<GameObject>();
    public List<GameObject> getBoxs = new List<GameObject>();//RoundEditorController����
    // Start is called before the first frame update
    void Start()
    {
        box = Resources.Load<GameObject>("RoundEditor/BoxEdge");



        spriteRenderer = GetComponent<SpriteRenderer>();
        random = Random.Range(0, sprites.Count);
        spriteRenderer.sprite = sprites[random];
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSelectSprite)
        {
            if (randomClock < 0)
            {
                randomClock = randomClockMax;
                int ran = Random.Range(0, sprites.Count);
                while (ran == random)
                {
                    ran = Random.Range(0, sprites.Count);
                }
                random = ran;
                spriteRenderer.sprite = sprites[ran];

                dir = Random.Range(-1, 1);
                if (dir == 0)
                {
                    dir = 1;
                    euler = Random.Range(0, 360f);
                }
                else
                {
                    euler = Random.Range(-360f, 0);
                }

            }
            else
            {
                spriteRenderer.color = Color.white * randomClock / randomClockMax * 2;
                randomClock -= Time.deltaTime;
            }
            if (euler > 360)
            {
                euler -= 360;
            }
            else if (euler < -360)
            {
                euler += 360;
            }
            euler += Time.deltaTime * spinSpeed * dir;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, euler));
        }
        else
        {
            transform.rotation = Quaternion.identity;
            spriteRenderer.color = Color.white;
        }

    }



    //-----����ز���-----

    /// <summary>
    /// ��ʼ��/�������
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(box, transform);
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
        availbleBox.Enqueue(gameObject);
    }
    /// <summary>
    /// ϲ����� box
    /// </summary>
    public GameObject GetFromPool()
    {
        if (availbleBox.Count == 0)
            FillPool();

        var box = availbleBox.Dequeue();

        box.SetActive(true);
        return box;
    }
}
