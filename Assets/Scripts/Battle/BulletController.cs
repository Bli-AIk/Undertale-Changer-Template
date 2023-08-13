using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Bullet controller
/// </summary>
public class BulletController : MonoBehaviour
{
    public string typeName;

    public SpriteRenderer spriteRenderer;
    public List<BoxCollider2D> boxColliderList = new List<BoxCollider2D>();
    public List<Vector2> boxColliderSizes = new List<Vector2>();
    public List<int> boxHitList = new List<int>();
    public BattleControl.BulletColor bulletColor;//Colors containing attributes. Read enum BulletColor in BattleControl

    public FollowMode followMode;
    //public bool useExtra;
    //public Collider2D extra;
    /// <summary>
    /// Set how the collision box follows the scaling of SpriteRenderer.
    /// CutFollow: Subtract the data stored in boxColliderSizes;
    /// NoFollow:Not following.
    /// FullFollow:Fully follow, which enables automatic splicing of the box collider.
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
    void Start()
    {
        //if (useExtra) 
        //    extra = GetComponent<Collider2D>();
    }
    /// <summary>
    /// Initialize the bullet (single collision mode).
    /// </summary>
    /// <param name="name">Set the name of the Obj of the bullet for easy lookup.</param>
    /// <param name="typeName">Set the type name of the bullet. If the type name is consistent with the current bullet, the original collision related parameters will be retained, and vice versa, it will be cleared.</param>
    /// <param name="layer">The player is 100, and the edge of the battle box is 50. The above data can be referred to.</param>
    /// <param name="sprite">Usually imported within Resources.</param>
    /// <param name="size">To set the size of the judgment box, multiple lists can be set, but in most cases, it is necessary to avoid overlapping them. (Set to (0,0) in NoFollow case, it will automatically synchronize with the sprite size)</param>
    /// <param name="offset">Set the judgment box offset, and the List size must be equal to the sizes.</param>
    /// <param name="hit">Set the collision box damage, and the List size must be equal to the sizes.</param>
    /// <param name="followMode">Set the follow mode for the collision box.</param>
    /// <param name="startMask">Set the mask mode.</param>
    /// <param name="bulletColor">Set bullet attribute color data</param>
    /// <param name="startPosition">Set the starting position (relative coordinates)¡£</param>
    /// <param name="startRotation">To set the rotation angle, usually only the Z-axis needs to be changed.</param>
    /// <param name="startScale">If the bullet does not need to be stretched, StartScale is generally set to (1,1,1). When Z is detected as 0, it will return to (1,1,1).</param>
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
    /// Initialize the bullet (Loop generated box collision mode).  
    /// </summary>
    /// <param name="name">Set the name of the Obj of the bullet for easy lookup.</param>
    /// <param name="typeName">Set the type name of the bullet. If the type name is consistent with the current bullet, the original collision related parameters will be retained, and vice versa, it will be cleared.</param>
    /// <param name="layer">The player is 100, and the edge of the battle box is 50. The above data can be referred to.</param>
    /// <param name="sprite">Usually imported within Resources.</param>
    /// <param name="sizes">To set the size of the judgment box, multiple lists can be set, but in most cases, it is necessary to avoid overlapping them. (Set to (0,0) in NoFollow case, it will automatically synchronize with the sprite size)</param>
    /// <param name="offsets">Set the judgment box offset, and the List size must be equal to the sizes.</param>
    /// <param name="hits">Set the collision box damage, and the List size must be equal to the sizes.</param>
    /// <param name="followMode">Set the follow mode for the collision box.</param>
    /// <param name="startMask">Set the mask mode.</param>
    /// <param name="bulletColor">Set bullet attribute color data</param>
    /// <param name="startPosition">Set the starting position (relative coordinates)¡£</param>
    /// <param name="startRotation">To set the rotation angle, usually only the Z-axis needs to be changed.</param>
    /// <param name="startScale">If the bullet does not need to be stretched, StartScale is generally set to (1,1,1). When Z is detected as 0, it will return to (1,1,1).</param>

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
        //Loop generated box collision
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
    private void OnTriggerStay2D(Collider2D collision)//Injury judgment
    {
        if (collision.transform.CompareTag("Player") && collision.name == "CheckCollider") 
        {
            //if(!useExtra)
            for (int i = 0; i < boxColliderList.Count; i++)
            {
                if (boxColliderList[i].IsTouching(collision))
                {
                    if (bulletColor == BattleControl.BulletColor.white
                        || (bulletColor == BattleControl.BulletColor.orange && !MainControl.instance.battlePlayerController.isMoveing)
                        || (bulletColor == BattleControl.BulletColor.blue && MainControl.instance.battlePlayerController.isMoveing))
                    {
                        HitPlayer(i);
                        if (!MainControl.instance.OverworldControl.noSFX)
                                MainControl.instance.battlePlayerController.hitVolume.weight = 1;
                    }
                    break;
                }
            }
            /*
            else if (extra.IsTouching(collision))
            {
                if (bulletColor == BattleControl.BulletColor.white
                    || (bulletColor == BattleControl.BulletColor.orange && !MainControl.instance.battlePlayerController.isMoveing)
                    || (bulletColor == BattleControl.BulletColor.blue && MainControl.instance.battlePlayerController.isMoveing))
                {
                    HitPlayer(0);
                    if (!MainControl.instance.OverworldControl.noSFX)
                        MainControl.instance.battlePlayerController.hitVolume.weight = 1;
                }
            }
            */
        }
    }

    void HitPlayer(int i)
    {
        if (MainControl.instance.PlayerControl.missTime < 0)
        {
            MainControl.instance.PlayerControl.hp -= boxHitList[i];
            MainControl.instance.PlayerControl.missTime = MainControl.instance.PlayerControl.missTimeMax;
            AudioController.instance.GetFx(5, MainControl.instance.AudioControl.fxClipUI);

            MainControl.instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);


            float r = UnityEngine.Random.Range(0.025f, 0.05f);
            Vector3 v3 = new Vector3(r * MainControl.instance.Get1Or_1(), r * MainControl.instance.Get1Or_1());
            MainControl.instance.cameraShake.Shake(v3);
            MainControl.instance.cameraShake3D.Shake(v3);

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
