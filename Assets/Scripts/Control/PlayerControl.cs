using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player information
/// and some related settings
/// </summary>
[CreateAssetMenu(fileName = "PlayerControl", menuName = "PlayerControl")]
public class PlayerControl : ScriptableObject
{
    public int hp, hpMax, lv, exp, gold, wearAtk, wearDef, nextExp;
    public float missTime, missTimeMax;

    [Header("AT and DF shown in OW backpack will be -10")]
    public int atk;

    public int def;

    public string playerName;
    public List<int> myItems;
    //Player backpack data Storage number
    public int wearArm, wearArmor;
    public bool canMove;

    public float gameTime;

    [Header("Scene articulation store")]
    public string lastScene;

    public string saveScene;

    [Header("Enable debugging")]
    public bool isDebug;

    [Header("--debug mode setting--")]
    [Header("Blood lock")]
    public bool invincible;
    public bool keepInvincible;
}
