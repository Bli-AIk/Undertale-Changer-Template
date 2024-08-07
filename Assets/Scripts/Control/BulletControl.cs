using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleControl;
using static BulletController;

/// <summary>
/// 弹幕文件
/// </summary>
[CreateAssetMenu(fileName = "BulletControl", menuName = "UCT-BulletControl")]
public class BulletControl : ScriptableObject
{
    [Header("弹幕的种类名称")]
    public string typeName;

    [Header("弹幕默认的Obj名称")]
    public string objName;

    [Header("玩家为100，战斗框边缘为50。可参考")]
    public int layer;

    [Header("精灵")]
    public Sprite sprite;

    [Header("判定箱大小")]
    public List<Vector2> triggerSize = new List<Vector2>() { new Vector2(1, 1) };

    [Header("判定箱伤害")]
    public List<int> triggerHit = new List<int> { 1 };

    [Header("判定箱偏移")]
    public List<Vector2> triggerOffset = new List<Vector2>() { new Vector2() };

    [Header("起始相对坐标")]
    public Vector3 startPosition = new Vector3();

    [Header("起始旋转角度")]
    public Vector3 startRotation = new Vector3();

    [Header("起始拉伸")]
    public Vector3 startScale = new Vector3(1, 1, 1);

    [Header("属性颜色数据")]
    public BulletColor bulletColor = BulletColor.white;

    [Header("Sprite遮罩模式")]
    public SpriteMaskInteraction startMask = SpriteMaskInteraction.None;

    [Header("碰撞箱缩放模式")]
    public FollowMode triggerFollowMode = FollowMode.NoFollow;


}
