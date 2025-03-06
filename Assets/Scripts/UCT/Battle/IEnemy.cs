using System;
using System.Collections.Generic;
using UCT.Global.Core;

namespace UCT.Battle
{
    public interface IEnemy
    {
        IEnemyTurnNumber TurnGenerator { get; set; } 
        
        Action[] GetActOptions();
        MercyType[] MercyTypes { get; }
        Action[] GetActLikeOptions();

        IEnumerator<float> _EnemyTurns(List<ObjectPool> objectPools);
        public EnemyState state { get; set; }
        
        public int exp { get; }
        public int gold { get; }
    }

    /// <summary>
    /// 针对Mercy的选项种类
    /// </summary>
    public enum MercyType
    {
        /// <summary>
        /// 不会执行任何方法
        /// </summary>
        Null,
        /// <summary>
        /// 对可饶恕对象选择后饶恕
        /// </summary>
        Mercy,
        /// <summary>
        /// 概率执行逃跑方法
        /// </summary>
        Flee,
        /// <summary>
        /// 类似Act一样执行自定义方法
        /// </summary>
        ActLike,
    }
}