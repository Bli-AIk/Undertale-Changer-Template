using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家的信息
/// 以及一些相关的设置
/// </summary>
[CreateAssetMenu(fileName = "PlayerControl", menuName = "PlayerControl")]
public class PlayerControl : ScriptableObject
{
    public int hp, hpMax, lv, exp, gold, wearAtk, wearDef, nextExp;
    public float missTime, missTimeMax;

    [Header("OW背包内显示的AT和DF会-10")]
    public int atk;
    public int def;

    public string playerName;
    public List<int> myItems;//玩家背包数据 储存编号
    public int wearArm, wearArmor;
    public bool canMove;
    public Vector3 deadPos;

    [Header("场景衔接存储")]
    public Vector3 scenePos;
    public Vector2 animDirection;
    public string lastScene;
}
