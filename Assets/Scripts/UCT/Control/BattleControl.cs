using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    ///     战斗系统管理器，仅使用于战斗场景。
    /// </summary>
    [CreateAssetMenu(fileName = "BattleControl", menuName = "UCT-Disposable/BattleControl")]
    public class BattleControl : ScriptableObject
    {
        /// <summary>
        ///     弹幕颜色数据（原版特殊弹幕），非视觉颜色
        /// </summary>
        public enum BulletColor
        {
            White,
            Blue,
            Orange,
            Green
        }

        /// <summary>
        ///     玩家数据颜色
        /// </summary>
        public enum PlayerColor
        {
            Red,
            Orange,
            Yellow,
            Green,
            Cyan,
            Blue,
            Purple
        }

        [Header("敌人OBJ")]
        [Header("物体名会识别为敌人名称")] public List<GameObject> enemies; //敌方的Obj。

        [Header("战斗内UIText读取")]
        public string uiText;

        public List<string> uiTextSave;

        [Header("存储ACT选项和选择后文本")]
        public List<string> actSave; //4个一对应 根据enemies而排序

        [Header("存储MERCY选项和选择后文本")]
        public List<string> mercySave;

        [Header("按回合存储旁白")]
        public List<string> turnTextSave;

        [Header("存储敌人对话文件")]
        public List<string> turnDialogAsset; //直接在战斗场景内读取

        public List<Color> bulletColorList;

        public List<Color> playerColorList, playerMissColorList;
    }
}