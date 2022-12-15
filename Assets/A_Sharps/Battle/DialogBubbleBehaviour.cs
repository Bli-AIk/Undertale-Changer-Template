using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// ս���ڵ��˶Ի����ݿ���
/// </summary>
public class DialogBubbleBehaviour : MonoBehaviour
{
    public Vector2 position, size;
    public bool isBackRight;
    public float backY;
    SpriteRenderer sprite, spriteBack;
    public TypeWritter typeWritter;
    public TextMeshPro tmp;
    private void Awake()
    {

        typeWritter = transform.Find("Text").GetComponent<TypeWritter>();
        tmp = transform.Find("Text").GetComponent<TextMeshPro>();
    }
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        spriteBack = transform.Find("DialogBubbleBack").GetComponent<SpriteRenderer>();

        PositionChange();
    }

    // Update is called once per frame
    void Update()
    {
        tmp.text = typeWritter.endString;
    }

    /// <summary>
    /// �ı����ݵĴ�С�����Ҽ�ͷ��� ���и�ֵ ֮���
    /// </summary>
    public void PositionChange()
    {
        transform.localPosition = position;
        sprite.size = size;
        spriteBack.flipX = isBackRight;
        if (!isBackRight)
        {
            spriteBack.transform.localPosition = new Vector3(0.05f, backY, 0);
        }
        else
        {
            spriteBack.transform.localPosition = new Vector3(3.95f + sprite.size.x - 4, backY, 0);

        }
    }
}
