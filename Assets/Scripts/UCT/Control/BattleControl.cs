using System.Collections.Generic;
using UCT.Battle.BattleConfigs;
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

        public IBattleConfig BattleConfig;

        public List<GameObject> enemies;

        public List<Color> bulletColorList;

        public List<Color> playerColorList, playerMissColorList;

        [HideInInspector] public List<string> actSave;

        [HideInInspector] public List<string> mercySave;

        [HideInInspector] public List<string> enemiesNameSave;

        [HideInInspector] public List<string> turnTextSave;

        [HideInInspector] public List<string> turnDialogAsset;
    }
}