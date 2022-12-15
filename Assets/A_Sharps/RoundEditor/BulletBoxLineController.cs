using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
public class BulletBoxLineController : MonoBehaviour
{
    BulletController bulletController;
    SpriteRenderer spriteRenderer;
    public bool randomColor;
    public int setNum;
    private void Start()
    {
        bulletController = transform.parent.GetComponent<BulletController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (randomColor)
        {
            spriteRenderer.color = new Color(Random.Range(0, 0.5f), Random.Range(0.5f, 1), Random.Range(0, 0.5f), 1);
        }
        SetSize(setNum);
    }

    /// <summary>
    /// 继承父级第number个碰撞体内的数值
    /// </summary>
    public void SetSize(int number)
    {
        spriteRenderer.size = bulletController.boxColliderList[number].size;
        transform.localPosition = bulletController.boxColliderList[number].offset;

    }


}
