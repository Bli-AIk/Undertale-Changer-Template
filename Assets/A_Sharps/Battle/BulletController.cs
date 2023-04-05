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
    public List<Vector2> boxColliderSizes = new List<Vector2>();
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//含有属性的颜色 读取BattleControl中的enum BulletColor

    public FollowMode followMode;
    public bool useExtra;
    public Collider2D extra;
    public TweenRotationCorrection tweenRotationCorrection;
    /// <summary>
    /// 设置碰撞箱跟随SpriteRenderer缩放的模式。
    /// CutFollow:切去boxColliderSizes内存储的数据；
    /// NoFollow:不跟随缩放。
    /// FullFollow:完全跟随缩放，因此你不需要设置boxColliderSizes。这一般不会用到；
    /// </summary>
    public enum FollowMode
    {
        CutFollow,
        NoFollow,
        FullFollow,
    }

    void Start()
    {
        if (tweenRotationCorrection == null)
            tweenRotationCorrection = transform.GetComponent<TweenRotationCorrection>();

        if (useExtra) 
            extra = GetComponent<Collider2D>();
    }
    /// <summary>
    /// 初始化弹幕。
    /// </summary>
    /// <param name="name">设置弹幕的Obj的名称，以便查找。</param>
    /// <param name="layer">基础值建议设为50（玩家为100）。</param>
    /// <param name="sprite">一般在Resources内导入。</param>
    /// <param name="startMask">设置Sprite遮罩模式。</param>
    /// <param name="bulletColor">设置弹幕属性颜色数据</param>
    /// <param name="startPosition">设置起始位置（相对坐标）。</param>
    /// <param name="startRotation">设置旋转角度，一般只需更改Z轴。</param>
    /// <param name="startScale">若弹幕不需拉伸，StartScale一般设置为(0.4f, 0.4f, 0.4f) 或 Vector3.zero * 0.4f。</param>
    /// <param name="sizes">设置判定箱大小，可设定多个List，但多数情况下需要避免其重叠。</param>
    /// <param name="offsets">设定判定箱偏移，List大小必须与sizes相等。</param>
    /// <param name="hits">设定碰撞箱伤害，List大小必须与sizes相等。</param>
    /// <param name="followMode">设置碰撞箱跟随SpriteRenderer缩放的模式。</param>
    public void SetBullet(string name, int layer, Sprite sprite, SpriteMaskInteraction startMask, BattleControl.BulletColor bulletColor,
        Vector3 startPosition, Vector3 startRotation, Vector3 startScale,  List<Vector2> sizes, List<Vector2> offsets, List<int> hits, FollowMode followMode = FollowMode.NoFollow)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        gameObject.name = name;

        spriteRenderer.sortingOrder = layer;

        this.bulletColor = bulletColor;
        spriteRenderer.color = MainControl.instance.BattleControl.bulletColorList[(int)this.bulletColor];
        
        transform.localPosition = startPosition;

        if (tweenRotationCorrection == null)
            tweenRotationCorrection = transform.GetComponent<TweenRotationCorrection>();

        tweenRotationCorrection.euler = startRotation;

        transform.localScale = startScale;


        spriteRenderer.sprite = sprite;
        SetMask(startMask);


        for (int i = 0; i < boxColliderList.Count; i++)
        {
            Destroy(boxColliderList[0]);
            boxColliderList.RemoveAt(0);
        }

        boxColliderSizes.Clear();
        boxHitList.Clear();

        boxColliderSizes = sizes;
        boxHitList = hits;
        //循环生成box碰撞
        for (int i = 0; i < sizes.Count; i++)
        {
            BoxCollider2D save = gameObject.AddComponent<BoxCollider2D>();
            save.isTrigger = true;
            if (followMode != FollowMode.CutFollow)
                save.size = boxColliderSizes[i];
            else
                save.size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];

            save.offset = offsets[i];
            
            boxColliderList.Add(save);
        }
    }
    private void Update()
    {
        if (followMode != FollowMode.NoFollow)
        {
            for (int i = 0; i < boxColliderList.Count; i++)
            {
                switch (followMode)
                {
                    case FollowMode.FullFollow:
                        boxColliderList[i].size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size;
                        break;
                    case FollowMode.CutFollow:
                        boxColliderList[i].size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];
                        break;
                }

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
    public void SetMask(SpriteMaskInteraction spriteMaskInteraction)
    {
        spriteRenderer.maskInteraction = spriteMaskInteraction;
    }
    private void OnDisable()
    {
        name = "Bullet";
    }
}
