using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹幕控制器
/// </summary>
public class BulletController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public List<BoxCollider2D> boxColliderList = new List<BoxCollider2D>();
    public List<Vector2> boxColliderSizes = new List<Vector2>();//给Follow用的（（
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//含有属性的颜色 读取BattleControl中的enum BulletColor
    public bool followMode;

    public bool useExtra;
    public Collider2D extra;

    void Start()
    {
        if (useExtra) 
            extra = GetComponent<Collider2D>();
    }
  
    /// <summary>
    /// 初始化
    /// </summary>
    public void SetBullet(string name, string layer, string color, string startPosition, string startRotation, string startScale, string startString)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.name = name;
        spriteRenderer.sortingOrder = int.Parse(layer);
        bulletColor = (BattleControl.BulletColor)Enum.Parse(typeof(BattleControl.BulletColor), color);
        spriteRenderer.color = MainControl.instance.BattleControl.bulletColorList[(int)bulletColor];
        transform.localPosition = MainControl.instance.StringVector2ToRealVector2(startPosition, transform.localPosition);
        transform.GetComponent<TweenRotationCorrection>().euler = new Vector3(0, 0, float.Parse(startRotation));
        transform.localScale = MainControl.instance.StringVector2ToRealVector2(startScale, transform.localScale);


        List<string> lister = new List<string>();
        MainControl.instance.MaxToOneSon(startString, lister);
        spriteRenderer.sprite = Resources.Load<Sprite>(lister[2]);
        SetMask(lister[3]);
        followMode = bool.Parse(lister[4]);
        int j = 0;

        for (int i = 0; i < boxColliderList.Count; i++)
        {
            Destroy(boxColliderList[0]);
            boxColliderList.RemoveAt(0);
        }

        boxColliderSizes.Clear();
        boxHitList.Clear();


        //4开始后 循环生成box碰撞
        for (int i = 5; i < lister.Count; i++)
        {
            switch (j)
            {
                case 0://size
                    boxColliderList.Add(gameObject.AddComponent<BoxCollider2D>());
                    boxColliderList[boxColliderList.Count - 1].isTrigger = true;
                    if (!followMode)
                        boxColliderList[boxColliderList.Count - 1].size = MainControl.instance.StringVector2ToRealVector2(lister[i], new Vector3());
                    else
                        boxColliderSizes.Add(MainControl.instance.StringVector2ToRealVector2(lister[i], new Vector3()));

                    goto default;

                case 1://offset
                    boxColliderList[boxColliderList.Count - 1].offset = MainControl.instance.StringVector2ToRealVector2(lister[i], new Vector3());
                    goto default;

                case 2:
                    j = 0;
                    boxHitList.Add(int.Parse(lister[i]));
                    break;

                default:
                    j++;
                    break;
            }


        }
    }
    private void Update()
    {
        if (followMode)
        {
            for (int i = 0; i < boxColliderList.Count; i++)
            {
                boxColliderList[i].size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)//伤害判定
    {
        if (collision.transform.tag == "Player") 
        {
            if(!useExtra)
            for (int i = 0; i < boxColliderList.Count; i++)
            {
                if (boxColliderList[i].IsTouching(collision))
                {
                    BattlePlayerController battlePlayerController = collision.GetComponent<BattlePlayerController>();
                    if (bulletColor == BattleControl.BulletColor.white
                        || (bulletColor == BattleControl.BulletColor.orange && !battlePlayerController.isMoveing)
                        || (bulletColor == BattleControl.BulletColor.blue && battlePlayerController.isMoveing))
                    {
                        HitPlayer(i);
                        if (!MainControl.instance.OverwroldControl.noSFX)
                            battlePlayerController.hitVolume.weight = 1;
                    }
                    break;
                }
            }
            else if (extra.IsTouching(collision))
            {
                BattlePlayerController battlePlayerController = collision.GetComponent<BattlePlayerController>();
                if (bulletColor == BattleControl.BulletColor.white
                    || (bulletColor == BattleControl.BulletColor.orange && !battlePlayerController.isMoveing)
                    || (bulletColor == BattleControl.BulletColor.blue && battlePlayerController.isMoveing))
                {
                    HitPlayer(0);
                    if (!MainControl.instance.OverwroldControl.noSFX)
                        battlePlayerController.hitVolume.weight = 1;
                }
            }
        }
    }

    void HitPlayer(int i)
    {
        if (MainControl.instance.PlayerControl.missTime < 0)
        {
            MainControl.instance.PlayerControl.hp -= boxHitList[i];
            MainControl.instance.PlayerControl.missTime = MainControl.instance.PlayerControl.missTimeMax;
            AudioController.instance.GetFx(5, MainControl.instance.AudioControl.fxClipUI);
           
        }
    }
    public void SetMask(string set)
    {
        switch (set)//遮罩
        {
            case "None":
                spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                break;

            case "In":
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                break;

            case "Out":
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                break;

            default:
                goto case "None";
        }
    }
    private void OnDisable()
    {
        name = "Bullet";
    }
}
