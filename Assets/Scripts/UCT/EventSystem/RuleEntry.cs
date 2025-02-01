using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统的Rule条目，被其他Event调用，同时可以触发其他Event与执行具体方法。
    /// </summary>
    [Serializable]
    public struct RuleEntry
    {
        /// <summary>
        ///     该Rule的名称
        /// </summary>
        public string name;

        /// <summary>
        ///     触发该Rule的Event名称
        /// </summary>
        public List<string> triggeredBy;

        /// <summary>
        ///     该Rule触发的Event名称
        /// </summary>
        public List<string> triggers;

        /// <summary>
        ///     该Rule的优先级
        /// </summary>
        public RulePriority rulePriority;
        //TODO:实装优先级

        /// <summary>
        ///     该Rule执行的方法名
        /// </summary>
        public List<string> methodNames;
        
        /// <summary>
        ///     该Rule执行的方法传入的第一个字符串形参
        /// </summary>
        public List<string> firstStringParams;

        /// <summary>
        ///     该Rule执行的方法传入的第二个字符串形参
        /// </summary>
        public List<string> secondStringParams;
        
        /// <summary>
        ///     该Rule执行的方法传入的第三个字符串形参
        /// </summary>
        public List<string> thirdStringParams;
        
        /// <summary>
        ///     是否使用该Rule执行的方法联动触发的Event
        /// </summary>
        public List<bool> useMethodEvents;
        
        /// <summary>
        ///     该Rule执行的方法联动触发的Event
        /// </summary>
        public List<string> methodEvents;

        /// <summary>
        ///     是否使用该Rule的Fact判断组
        /// </summary>
        public bool useRuleCriterion;
        
        /// <summary>
        ///     该Rule的Fact判断组
        /// </summary>
        public RuleCriterion ruleCriterion;
        
        /// <summary>
        ///     该Rule修改的Fact值
        /// </summary>
        public List<FactModification> factModifications;
    }

    /// <summary>
    ///     用于传递修改Fact值参数的结构体。
    /// </summary>
    [Serializable]
    public struct FactModification
    {
        public FactEntry fact;
        public Operation operation;
        public int number;

        public enum Operation
        {
            Change,
            Add,
            Subtract,
            Multiply,
            Divide
        }
    }

    /// <summary>
    ///     Rule的规则，用于判断fact和数值之间的关系。
    ///     请特别注意：倘若criteria包含值，那么会忽略除了criteria和result之外的所有值。
    /// </summary>
    [Serializable]
    public struct RuleCriterion
    {
        public bool isResultReversed;
        public FactEntry fact;
        public CriteriaCompare compare;
        public int detection;

        public RuleLogicalOperation operation;

        public List<RuleCriterion> criteria;

        public bool GetResult()
        {
            return GetResult(isResultReversed, fact, compare, detection, criteria);
        }

        /// <summary>
        ///     封装，为Editor调用
        /// </summary>
        public static bool GetResult(bool isResultReversed, FactEntry fact,
            CriteriaCompare compare, int detection, List<RuleCriterion> criteria)
        {
            if (criteria.Count == 0)
            {
                var result = compare switch
                {
                    CriteriaCompare.GreaterThan => fact.value > detection,
                    CriteriaCompare.GreaterThanOrEqual => fact.value >= detection,
                    CriteriaCompare.Equal => fact.value == detection,
                    CriteriaCompare.NotEqual => fact.value != detection,
                    CriteriaCompare.LessThanOrEqual => fact.value <= detection,
                    CriteriaCompare.LessThan => fact.value < detection,
                    _ => throw new ArgumentOutOfRangeException()
                };

                return isResultReversed ? !result : result;
            }

            List<bool> results = new();
            List<RuleLogicalOperation> operations = new();
            foreach (var criterion in criteria)
            {
                results.Add(criterion.GetResult());
                operations.Add(criterion.operation);
            }

            var finalResult = results[0];
            for (var i = 1; i < results.Count; i++)
            {
                switch (operations[i - 1])
                {
                    case RuleLogicalOperation.And:
                    {
                        finalResult = finalResult && results[i];
                        break;
                    }
                    case RuleLogicalOperation.Or:
                    {
                        finalResult = finalResult || results[i];
                        break;
                    }
                    case RuleLogicalOperation.None: break;
                    default: throw new ArgumentOutOfRangeException();
                }

                if (operations[i - 1] == RuleLogicalOperation.None) InvalidOperationLogError(i, "None");
                switch (operations[^1])
                {
                    case RuleLogicalOperation.And:
                    {
                        InvalidOperationLogError(i, "And");
                        break;
                    }
                    case RuleLogicalOperation.Or:
                    {
                        InvalidOperationLogError(i, "Or");
                        break;
                    }
                    case RuleLogicalOperation.None: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            return isResultReversed ? !finalResult : finalResult;
        }

        private static void InvalidOperationLogError(int i, string name)
        {
            Other.Debug.LogWarning(
                $"Invalid operation '{name}' at index {i - 1}. This will cause calculation anomalies.");
        }
    }

    [Serializable]
    public enum CriteriaCompare
    {
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        NotEqual,
        LessThanOrEqual,
        LessThan
    }

    [Serializable]
    public enum RuleLogicalOperation
    {
        None,
        And,
        Or
    }

    [Serializable]
    public enum RulePriority
    {
        Low,
        Medium,
        High
    }
}