using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹幕控制器
/// </summary>
public class BulletController : MonoBehaviour
{
    public string typeName;

    public SpriteRenderer spriteRenderer;
    public List<BoxCollider2D> boxColliderList = new List<BoxCollider2D>();
    public List<Vector2> boxColliderSizes = new List<Vector2>();
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//含有属性的颜色 读取BattleControl中的enum BulletColor

    public FollowMode followMode;

    //public bool useExtra;
    //public Collider2D extra;
    /// <summary>
    /// 设置碰撞箱跟随SpriteRenderer缩放的模式。
    /// CutFollow:切去boxColliderSizes内存储的数据；
    /// NoFollow:不跟随缩放。
    /// FullFollow:完全跟随缩放，即启用盒碰撞器的自动拼接。
    /// </summary>
    public enum FollowMode
    {
        CutFollow,
        NoFollow,
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //if (useExtra)
        //    extra = GetComponent<Collider2D>();
    }
    public void SetBullet(string bulletPathName,
        Vector3 startPosition = default, BattleControl.BulletColor bulletColor = default, SpriteMaskInteraction startMask = default, Vector3 startRotation = default, Vector3 startScale = default)
    {
        string path = "Assets/Bullets/" + bulletPathName;

        SetBullet((BulletControl)Resources.Load(path), startPosition, bulletColor, startMask, startRotation, startScale);
    }
    public void SetBullet(BulletControl bulletControl,
        Vector3 startPosition = default, BattleControl.BulletColor bulletColor = default, SpriteMaskInteraction startMask = default, Vector3 startRotation = default, Vector3 startScale = default)
    {
        Debug.LogWarning(startPosition);

        if (startPosition == default)
            startPosition = bulletControl.startPosition;
        if (bulletColor == default)
            bulletColor = bulletControl.bulletColor;
        if (startMask == default)
            startMask = bulletControl.startMask;
        if (startRotation == default)
            startRotation = bulletControl.startRotation;
        if (startScale == default)
            startScale = bulletControl.startScale;

        SetBullet(bulletControl.name, 
            bulletControl.typeName, 
            bulletControl.layer, 
            bulletControl.sprite,
            bulletControl.size, 
            bulletControl.hit, 
            bulletControl.offset,
            startPosition, 
            bulletColor, 
            startMask,
            startRotation,
            startScale,
            bulletControl.followMode);
    }

    /// <summary>
    /// 初始化弹幕（单个碰撞模式）。
    /// </summary>
    /// <param name="name">设置弹幕的Obj的名称，以便查找。</param>
    /// <param name="typeName">设置弹幕的种类名称，如果种类名称与当前的弹幕一致，则保留原有的碰撞相关参数，反之清空。</param>
    /// <param name="layer">玩家为100，战斗框边缘为50。可参考。</param>
    /// <param name="sprite">一般在Resources内导入。</param>
    /// <param name="size">设置判定箱大小，可设定多个List，但多数情况下需要避免其重叠。（NoFollow情况下设为(0,0)，会自动与sprite大小同步）</param>
    /// <param name="offset">设定判定箱偏移，List大小必须与sizes相等。</param>
    /// <param name="hit">设定碰撞箱伤害，List大小必须与sizes相等。</param>
    /// <param name="followMode">设置碰撞箱跟随SpriteRenderer缩放的模式。</param>
    /// <param name="startMask">设置Sprite遮罩模式。</param>
    /// <param name="bulletColor">设置弹幕属性颜色数据</param>
    /// <param name="startPosition">设置起始位置（相对坐标）。</param>
    /// <param name="startRotation">设置旋转角度，一般只需更改Z轴。</param>
    /// <param name="startScale">若弹幕不需拉伸，StartScale一般设置(1,1,1)。检测到Z为0时会归位到(1,1,1)。</param>
    public void SetBullet(
       string name,
       string typeName,
       int layer,
       Sprite sprite,
       Vector2 size,
       int hit,
       Vector2 offset,
       Vector3 startPosition = new Vector3(),
       BattleControl.BulletColor bulletColor = BattleControl.BulletColor.white,
       SpriteMaskInteraction startMask = SpriteMaskInteraction.None,
       Vector3 startRotation = new Vector3(),
       Vector3 startScale = new Vector3(),
       FollowMode followMode = FollowMode.NoFollow
       )
    {
        gameObject.name = name;

        spriteRenderer.sortingOrder = layer;

        this.bulletColor = bulletColor;
        spriteRenderer.color = MainControl.instance.BattleControl.bulletColorList[(int)this.bulletColor];

        transform.localPosition = startPosition;

        transform.rotation = Quaternion.Euler(startRotation);

        if (startScale.z == 0)
            startScale = Vector3.one;

        transform.localScale = startScale;

        spriteRenderer.sprite = sprite;
        SetMask(startMask);

        if (this.typeName != typeName)
            this.typeName = typeName;
        else
            return;

        for (int i = 0; i < boxColliderList.Count; i++)
        {
            Destroy(boxColliderList[0]);
            boxColliderList.RemoveAt(0);
        }

        boxColliderSizes.Clear();
        boxHitList.Clear();

        boxColliderSizes.Add(size);
        boxHitList.Add(hit);

        BoxCollider2D save = gameObject.AddComponent<BoxCollider2D>();
        save.isTrigger = true;
        if (followMode == FollowMode.NoFollow)
            save.size = boxColliderSizes[0];
        else
        {
            save.size = boxColliderList[0].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[0];
        }

        save.offset = offset;

        boxColliderList.Add(save);
    }

    /// <summary>
    /// 初始化弹幕（循环生成盒状碰撞模式）。
    /// </summary>
    /// <param name="name">设置弹幕的Obj的名称，以便查找。</param>
    /// <param name="typeName">设置弹幕的种类名称，如果种类名称与当前的弹幕一致，则保留原有的碰撞相关参数，反之清空。</param>
    /// <param name="layer">玩家为100，战斗框边缘为50。可参考。</param>
    /// <param name="sprite">一般在Resources内导入。</param>
    /// <param name="sizes">设置判定箱大小，可设定多个List，但多数情况下需要避免其重叠。（NoFollow情况下设为(0,0)，会自动与sprite大小同步）</param>
    /// <param name="offsets">设定判定箱偏移，List大小必须与sizes相等。</param>
    /// <param name="hits">设定碰撞箱伤害，List大小必须与sizes相等。</param>
    /// <param name="followMode">设置碰撞箱跟随SpriteRenderer缩放的模式。</param>
    /// <param name="startMask">设置Sprite遮罩模式。</param>
    /// <param name="bulletColor">设置弹幕属性颜色数据</param>
    /// <param name="startPosition">设置起始位置（相对坐标）。</param>
    /// <param name="startRotation">设置旋转角度，一般只需更改Z轴。</param>
    /// <param name="startScale">若弹幕不需拉伸，StartScale一般设置(1,1,1)。检测到Z为0时会归位到(1,1,1)。</param>

    public void SetBullet(
        string name,
        string typeName,
        int layer,
        Sprite sprite,
        List<Vector2> sizes,
        List<int> hits,
        List<Vector2> offsets,
        Vector3 startPosition = new Vector3(),
        BattleControl.BulletColor bulletColor = BattleControl.BulletColor.white,
        SpriteMaskInteraction startMask = SpriteMaskInteraction.None,
        Vector3 startRotation = new Vector3(),
        Vector3 startScale = new Vector3(),
        FollowMode followMode = FollowMode.NoFollow
        )
    {
        gameObject.name = name;

        spriteRenderer.sortingOrder = layer;

        this.bulletColor = bulletColor;
        spriteRenderer.color = MainControl.instance.BattleControl.bulletColorList[(int)this.bulletColor];

        transform.localPosition = startPosition;

        transform.rotation = Quaternion.Euler(startRotation);

        if (startScale.z == 0)
            startScale = Vector3.one;

        transform.localScale = startScale;

        SetMask(startMask);
        spriteRenderer.sprite = sprite;

        if (this.typeName != typeName)
            this.typeName = typeName;
        else
            return;

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
            if (followMode == FollowMode.NoFollow)
                save.size = boxColliderSizes[i];
            else
            {
                save.size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];
            }

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
                    case FollowMode.CutFollow:
                        boxColliderList[i].size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];
                        break;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)//伤害判定
    {
        if (collision.transform.CompareTag("Player") && collision.name.Substring(0, "CheckCollider".Length) == "CheckCollider")
        {
            //if(!useExtra)
            for (int i = 0; i < boxColliderList.Count; i++)
            {
                if (boxColliderList[i].IsTouching(collision))
                {
                    if (bulletColor == BattleControl.BulletColor.white
                        || (bulletColor == BattleControl.BulletColor.orange && !MainControl.instance.battlePlayerController.isMoving)
                        || (bulletColor == BattleControl.BulletColor.blue && MainControl.instance.battlePlayerController.isMoving))
                    {
                        HitPlayer(i);
                    }
                    break;
                }
            }
            /*
            else if (extra.IsTouching(collision))
            {
                if (bulletColor == BattleControl.BulletColor.white
                    || (bulletColor == BattleControl.BulletColor.orange && !MainControl.instance.battlePlayerController.isMoving)
                    || (bulletColor == BattleControl.BulletColor.blue && MainControl.instance.battlePlayerController.isMoving))
                {
                    HitPlayer(0);
                    if (!MainControl.instance.OverworldControl.noSFX)
                        MainControl.instance.battlePlayerController.hitVolume.weight = 1;
                }
            }
            */
        }
    }

    private void HitPlayer(int i)
    {
        if (MainControl.instance.PlayerControl.missTime < 0)
        {
            MainControl.instance.PlayerControl.hp -= boxHitList[i];
            MainControl.instance.PlayerControl.missTime = MainControl.instance.PlayerControl.missTimeMax;
            AudioController.instance.GetFx(5, MainControl.instance.AudioControl.fxClipUI);

            MainControl.instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);

            float r = Random.Range(0, 0.025f);
            Vector3 v3spin = MainControl.instance.RandomPointOnSphereSurface(-2.5f,2.5f,2.5f,new Vector3());
            MainControl.instance.cameraShake.Shake(new Vector3(r * MainControl.instance.Get1Or_1(), r * MainControl.instance.Get1Or_1(), 0), new Vector3(0, 0, v3spin.z), 4, 1f / 60f * 4f * 1.5f, "", Ease.OutElastic);
            MainControl.instance.cameraShake3D.Shake(new Vector3(r * MainControl.instance.Get1Or_1(), 0, r * MainControl.instance.Get1Or_1()), v3spin, 4, 1f / 60f * 4f * 1.5f, "3D CameraPoint", Ease.OutElastic);
            if (MainControl.instance.PlayerControl.hp <= 0)
                MainControl.instance.KillPlayer();


            if (!MainControl.instance.OverworldControl.noSFX)
                MainControl.instance.battlePlayerController.hitVolume.weight = 1;
        }
    }

    public void SetMask(SpriteMaskInteraction spriteMaskInteraction)
    {
        switch (spriteMaskInteraction)
        {
            case SpriteMaskInteraction.None:
                spriteRenderer.material.SetFloat("_OpenMask", 0);
                break;

            case SpriteMaskInteraction.VisibleInsideMask:
                spriteRenderer.material.SetFloat("_OpenMask", 1);
                spriteRenderer.material.SetFloat("_OutSide", 0);
                break;

            case SpriteMaskInteraction.VisibleOutsideMask:
                spriteRenderer.material.SetFloat("_OpenMask", 1);
                spriteRenderer.material.SetFloat("_OutSide", 1);
                break;

            default:
                break;
        }
    }

    private void OnDisable()
    {
        name = "Bullet";
    }
}