using System;
using System.Collections.Generic;
using UCT.Core;

namespace UCT.Battle.Enemies
{
    public interface IEnemy
    {
        IEnemyTurnNumber TurnGenerator { get; set; }
        MercyType[] MercyTypes { get; }
        public EnemyState state { get; set; }

        public int exp { get; }
        public int gold { get; }

        Action[] GetActOptions();
        Action[] GetActLikeOptions();

        IEnumerator<float> _EnemyTurns(ObjectPool bulletPool, ObjectPool boardPool);
    }

    /// <summary>
    ///     针对Mercy的选项种类
    /// </summary>
    public enum MercyType
    {
        /// <summary>
        ///     不会执行任何方法
        /// </summary>
        Null,

        /// <summary>
        ///     对可饶恕对象选择后饶恕
        /// </summary>
        Mercy,

        /// <summary>
        ///     概率执行逃跑方法
        /// </summary>
        Flee,

        /// <summary>
        ///     类似Act一样执行自定义方法
        /// </summary>
        ActLike
    }
}