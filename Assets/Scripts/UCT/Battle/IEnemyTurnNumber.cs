using System;
using System.Collections.Generic;
using System.Linq;

namespace UCT.Battle
{
    /// <summary>
    ///     表示敌人回合数生成器的接口。
    /// </summary>
    public interface IEnemyTurnNumber
    {
        /// <summary>
        ///     获取下一个回合数值。
        /// </summary>
        /// <returns>返回下一个回合数。</returns>
        int GetNextValue();
    }

    /// <summary>
    ///     通过加权随机选择下一个回合数。
    /// </summary>
    public class WeightedRandomTurnNumber : IEnemyTurnNumber
    {
        private readonly Random _random = new();
        private readonly Dictionary<int, int> _weights;

        /// <summary>
        ///     初始化 <see cref="WeightedRandomTurnNumber" /> 类，默认所有值的权重为 1。
        /// </summary>
        /// <param name="values">可能的回合数列表。</param>
        public WeightedRandomTurnNumber(HashSet<int> values)
        {
            _weights = values.ToDictionary(v => v, _ => 1);
        }

        /// <summary>
        ///     初始化 <see cref="WeightedRandomTurnNumber" /> 类，并设置各个值的权重。
        /// </summary>
        /// <param name="weights">键值对，键为可能的回合数，值为其权重。</param>
        public WeightedRandomTurnNumber(Dictionary<int, int> weights)
        {
            _weights = weights;
        }

        /// <summary>
        ///     生成一个基于权重的随机回合数。
        /// </summary>
        /// <returns>返回生成的回合数。</returns>
        public int GetNextValue()
        {
            var totalWeight = _weights.Values.Sum();
            var roll = _random.Next(totalWeight);
            var sum = 0;

            foreach (var pair in _weights)
            {
                sum += pair.Value;
                if (roll < sum)
                {
                    return pair.Key;
                }
            }

            return _weights.Keys.Last();
        }
    }

    /// <summary>
    ///     按照固定顺序循环返回回合数。
    /// </summary>
    public class CyclicTurnNumber : IEnemyTurnNumber
    {
        private readonly List<int> _values;
        private int _index;

        /// <summary>
        ///     初始化 <see cref="CyclicTurnNumber" /> 类。
        /// </summary>
        /// <param name="values">按顺序循环的回合数列表。</param>
        public CyclicTurnNumber(List<int> values)
        {
            _values = values;
        }

        /// <summary>
        ///     获取下一个回合数，按照顺序循环。
        /// </summary>
        /// <returns>返回下一个回合数。</returns>
        public int GetNextValue()
        {
            var value = _values[_index];
            _index = (_index + 1) % _values.Count;
            return value;
        }
    }

    /// <summary>
    ///     始终返回固定的回合数。
    /// </summary>
    public class FixedTurnNumber : IEnemyTurnNumber
    {
        private int _value;

        /// <summary>
        ///     初始化 <see cref="FixedTurnNumber" /> 类。
        /// </summary>
        /// <param name="value">固定的回合数值。</param>
        public FixedTurnNumber(int value)
        {
            _value = value;
        }

        /// <summary>
        ///     获取固定的回合数。
        /// </summary>
        /// <returns>返回固定的回合数。</returns>
        public int GetNextValue()
        {
            return _value;
        }

        /// <summary>
        ///     设置固定的回合数值。
        /// </summary>
        /// <param name="value">要设置的回合数值。</param>
        public void SetValue(int value)
        {
            _value = value;
        }
    }
}