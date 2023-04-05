using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ļ������
/// </summary>
public class BulletController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public List<BoxCollider2D> boxColliderList = new List<BoxCollider2D>();
    public List<Vector2> boxColliderSizes = new List<Vector2>();
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//�������Ե���ɫ ��ȡBattleControl�е�enum BulletColor

    public FollowMode followMode;
    public bool useExtra;
    public Collider2D extra;
    public TweenRotationCorrection tweenRotationCorrection;
    /// <summary>
    /// ������ײ�����SpriteRenderer���ŵ�ģʽ��
    /// CutFollow:��ȥboxColliderSizes�ڴ洢�����ݣ�
    /// NoFollow:���������š�
    /// FullFollow:��ȫ�������ţ�����㲻��Ҫ����boxColliderSizes����һ�㲻���õ���
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
    /// ��ʼ����Ļ��
    /// </summary>
    /// <param name="name">���õ�Ļ��Obj�����ƣ��Ա���ҡ�</param>
    /// <param name="layer">����ֵ������Ϊ50�����Ϊ100����</param>
    /// <param name="sprite">һ����Resources�ڵ��롣</param>
    /// <param name="startMask">����Sprite����ģʽ��</param>
    /// <param name="bulletColor">���õ�Ļ������ɫ����</param>
    /// <param name="startPosition">������ʼλ�ã�������꣩��</param>
    /// <param name="startRotation">������ת�Ƕȣ�һ��ֻ�����Z�ᡣ</param>
    /// <param name="startScale">����Ļ�������죬StartScaleһ������Ϊ(0.4f, 0.4f, 0.4f) �� Vector3.zero * 0.4f��</param>
    /// <param name="sizes">�����ж����С�����趨���List���������������Ҫ�������ص���</param>
    /// <param name="offsets">�趨�ж���ƫ�ƣ�List��С������sizes��ȡ�</param>
    /// <param name="hits">�趨��ײ���˺���List��С������sizes��ȡ�</param>
    /// <param name="followMode">������ײ�����SpriteRenderer���ŵ�ģʽ��</param>
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
        //ѭ������box��ײ
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
    private void OnTriggerStay2D(Collider2D collision)//�˺��ж�
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
