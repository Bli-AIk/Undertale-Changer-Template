using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ļ������
/// </summary>
public class BulletController : MonoBehaviour
{
    public string typeName;

    public SpriteRenderer spriteRenderer;
    public List<BoxCollider2D> boxColliderList = new List<BoxCollider2D>();
    public List<Vector2> boxColliderSizes = new List<Vector2>();
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//�������Ե���ɫ ��ȡBattleControl�е�enum BulletColor

    public FollowMode followMode;

    //public bool useExtra;
    //public Collider2D extra;
    /// <summary>
    /// ������ײ�����SpriteRenderer���ŵ�ģʽ��
    /// CutFollow:��ȥboxColliderSizes�ڴ洢�����ݣ�
    /// NoFollow:���������š�
    /// FullFollow:��ȫ�������ţ������ú���ײ�����Զ�ƴ�ӡ�
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

    /// <summary>
    /// ��ʼ����Ļ��������ײģʽ����
    /// </summary>
    /// <param name="name">���õ�Ļ��Obj�����ƣ��Ա���ҡ�</param>
    /// <param name="typeName">���õ�Ļ���������ƣ�������������뵱ǰ�ĵ�Ļһ�£�����ԭ�е���ײ��ز�������֮��ա�</param>
    /// <param name="layer">���Ϊ100��ս�����ԵΪ50���ɲο���</param>
    /// <param name="sprite">һ����Resources�ڵ��롣</param>
    /// <param name="size">�����ж����С�����趨���List���������������Ҫ�������ص�����NoFollow�������Ϊ(0,0)�����Զ���sprite��Сͬ����</param>
    /// <param name="offset">�趨�ж���ƫ�ƣ�List��С������sizes��ȡ�</param>
    /// <param name="hit">�趨��ײ���˺���List��С������sizes��ȡ�</param>
    /// <param name="followMode">������ײ�����SpriteRenderer���ŵ�ģʽ��</param>
    /// <param name="startMask">����Sprite����ģʽ��</param>
    /// <param name="bulletColor">���õ�Ļ������ɫ����</param>
    /// <param name="startPosition">������ʼλ�ã�������꣩��</param>
    /// <param name="startRotation">������ת�Ƕȣ�һ��ֻ�����Z�ᡣ</param>
    /// <param name="startScale">����Ļ�������죬StartScaleһ������(1,1,1)����⵽ZΪ0ʱ���λ��(1,1,1)��</param>
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
    /// ��ʼ����Ļ��ѭ�����ɺ�״��ײģʽ����
    /// </summary>
    /// <param name="name">���õ�Ļ��Obj�����ƣ��Ա���ҡ�</param>
    /// <param name="typeName">���õ�Ļ���������ƣ�������������뵱ǰ�ĵ�Ļһ�£�����ԭ�е���ײ��ز�������֮��ա�</param>
    /// <param name="layer">���Ϊ100��ս�����ԵΪ50���ɲο���</param>
    /// <param name="sprite">һ����Resources�ڵ��롣</param>
    /// <param name="sizes">�����ж����С�����趨���List���������������Ҫ�������ص�����NoFollow�������Ϊ(0,0)�����Զ���sprite��Сͬ����</param>
    /// <param name="offsets">�趨�ж���ƫ�ƣ�List��С������sizes��ȡ�</param>
    /// <param name="hits">�趨��ײ���˺���List��С������sizes��ȡ�</param>
    /// <param name="followMode">������ײ�����SpriteRenderer���ŵ�ģʽ��</param>
    /// <param name="startMask">����Sprite����ģʽ��</param>
    /// <param name="bulletColor">���õ�Ļ������ɫ����</param>
    /// <param name="startPosition">������ʼλ�ã�������꣩��</param>
    /// <param name="startRotation">������ת�Ƕȣ�һ��ֻ�����Z�ᡣ</param>
    /// <param name="startScale">����Ļ�������죬StartScaleһ������(1,1,1)����⵽ZΪ0ʱ���λ��(1,1,1)��</param>

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
        //ѭ������box��ײ
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

    private void OnTriggerStay2D(Collider2D collision)//�˺��ж�
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
                spriteRenderer.material.SetFloat("_IsMask", 0);
                break;

            case SpriteMaskInteraction.VisibleInsideMask:
                spriteRenderer.material.SetFloat("_IsMask", 1);
                spriteRenderer.material.SetFloat("_IsOutSide", 0);
                break;

            case SpriteMaskInteraction.VisibleOutsideMask:
                spriteRenderer.material.SetFloat("_IsMask", 1);
                spriteRenderer.material.SetFloat("_IsOutSide", 1);
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