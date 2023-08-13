using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player Information
/// And some related settings
/// </summary>
[CreateAssetMenu(fileName = "PlayerControl", menuName = "PlayerControl")]
public class PlayerControl : ScriptableObject
{
    public int hp, hpMax, lv, exp, gold, wearAtk, wearDef, nextExp;
    public float missTime, missTimeMax;

    [Header("The AT and DF displayed in the Overworld backpack will be reduced by 10")]
    public int atk;
    public int def;

    public string playerName;
    public List<int> myItems;//玩家背包数据 储存编号
    public int wearArm, wearArmor;
    public bool canMove;

    public float gameTime;


    [Header("Scene switching storage")]
    public string lastScene;
    public string saveScene;


    [Header("Open debug")]
    public bool isDebug;
    [Header("--Debug mode settings--")]
    [Header("Invincible")]
    public bool invincible;
}
