using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Control
{
    /// <summary>
    ///     玩家的信息
    ///     以及一些相关的设置
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerControl", menuName = "UCT-Disposable/PlayerControl")]
    public class PlayerControl : ScriptableObject
    {
        public int hp, hpMax, lv, exp, gold, nextExp;
        public float missTime, missTimeMax;

        [Header("OW背包内显示的AT和DF会-10")]
        public int atk;

        public int def;

        public string playerName;
        [FormerlySerializedAs("myItems")] public List<string> items; //玩家背包数据 储存编号
        public string wearWeapon;
        public string wearArmor;
        public bool canMove;

        public float gameTime;


        [Header("玩家相关")]
        public Vector3 playerLastPos;
        public Vector3 playerLastPosInBattle;

        [Header("场景衔接存储")]
        public string lastScene;

        public string saveScene;

        [Header("开启调试")]
        public bool isDebug;

        [Header("--调试模式设定--")]
        [Header("锁血")] public bool invincible;

        public bool keepInvincible;
    }
}