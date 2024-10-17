using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    /// 战斗系统管理器，仅使用于战斗场景。
    /// </summary>
    [CreateAssetMenu(fileName = "BattleControl", menuName = "UCT-Disposable/BattleControl")]
    public class BattleControl : ScriptableObject
    {
        [Header("敌人OBJ")]
        [Header("物体名会识别为敌人名称")]
        public List<GameObject> enemies;//敌方的Obj。

        [Header("HP 偶为目前血量 奇为最大Max 下同")]
        public List<int> enemiesHp;

        public List<int> enemiesATK, enemiesDEF;

        [Header("战斗内UIText读取")]
        public string uiText;

        public List<string> uiTextSave;

        [Header("存储ACT选项和选择后文本")]
        public List<string> actSave;//4个一对应 根据enemies而排序

        [Header("存储MERCY选项和选择后文本")]
        public List<string> mercySave;

        [Header("按回合存储旁白")]
        public List<string> turnTextSave;

        [Header("存储敌人对话文件")]
        public List<string> turnDialogAsset;//直接在战斗场景内读取

        public List<TextAsset> otherDialogAsset;

        /// <summary>
        /// 弹幕颜色数据（原版特殊弹幕），非视觉颜色
        /// </summary>
        public enum BulletColor
        {
            white,
            blue,
            orange,
            green
        }

        public List<Color> bulletColorList;

        /// <summary>
        /// 玩家数据颜色
        /// </summary>
        public enum PlayerColor
        {
            red,
            orange,
            yellow,
            green,
            cyan,
            blue,
            purple,
        }

        public List<Color> playerColorList, playerMissColorList;

        [Header("项目附加")]
        public int randomTurnDir;
    }
}