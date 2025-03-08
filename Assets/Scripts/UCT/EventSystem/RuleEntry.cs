using System;
using System.Collections.Generic;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统的Rule条目，被其他Event调用，同时可以触发其他Event与执行具体方法。
    /// </summary>
    [Serializable]
    public struct RuleEntry : IEquatable<RuleEntry>
    {
        /// <summary>
        ///     该Rule的名称
        /// </summary>
        public string name;

        /// <summary>
        ///     是否让triggeredBy使用全局的EventTable
        /// </summary>
        public bool isGlobalTriggeredBy;

        /// <summary>
        ///     触发该Rule的Event名称
        /// </summary>
        public List<string> triggeredBy;

        /// <summary>
        ///     是否让triggers使用全局的EventTable
        /// </summary>
        public bool isGlobalTriggers;

        /// <summary>
        ///     该Rule触发的Event名称
        /// </summary>
        public List<string> triggers;

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
        ///     是否让useMethodEvents使用全局的EventTable
        /// </summary>
        public List<bool> isGlobalMethodEvents;

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
        ///     是否让factModifications的所有Fact使用全局的EventTable
        /// </summary>
        public List<bool> isGlobalFactModifications;

        /// <summary>
        ///     该Rule修改的Fact值
        /// </summary>
        public List<FactModification> factModifications;

        public bool Equals(RuleEntry other)
        {
            return name == other.name && isGlobalTriggeredBy == other.isGlobalTriggeredBy &&
                   Equals(triggeredBy, other.triggeredBy) && isGlobalTriggers == other.isGlobalTriggers &&
                   Equals(triggers, other.triggers) &&
                   Equals(methodNames, other.methodNames) && Equals(firstStringParams, other.firstStringParams) &&
                   Equals(secondStringParams, other.secondStringParams) &&
                   Equals(thirdStringParams, other.thirdStringParams) &&
                   Equals(isGlobalMethodEvents, other.isGlobalMethodEvents) &&
                   Equals(useMethodEvents, other.useMethodEvents) && Equals(methodEvents, other.methodEvents) &&
                   useRuleCriterion == other.useRuleCriterion && ruleCriterion.Equals(other.ruleCriterion) &&
                   Equals(isGlobalFactModifications, other.isGlobalFactModifications) &&
                   Equals(factModifications, other.factModifications);
        }

        public override bool Equals(object obj)
        {
            return obj is RuleEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(name);
            hashCode.Add(isGlobalTriggeredBy);
            hashCode.Add(triggeredBy);
            hashCode.Add(isGlobalTriggers);
            hashCode.Add(triggers);
            hashCode.Add(methodNames);
            hashCode.Add(firstStringParams);
            hashCode.Add(secondStringParams);
            hashCode.Add(thirdStringParams);
            hashCode.Add(isGlobalMethodEvents);
            hashCode.Add(useMethodEvents);
            hashCode.Add(methodEvents);
            hashCode.Add(useRuleCriterion);
            hashCode.Add(ruleCriterion);
            hashCode.Add(isGlobalFactModifications);
            hashCode.Add(factModifications);
            return hashCode.ToHashCode();
        }
    }

    /// <summary>
    ///     用于传递修改Fact值参数的结构体。
    /// </summary>
    [Serializable]
    public struct FactModification : IEquatable<FactModification>
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

        public bool Equals(FactModification other)
        {
            return fact.Equals(other.fact) && operation == other.operation && number == other.number;
        }

        public override bool Equals(object obj)
        {
            return obj is FactModification other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fact, (int)operation, number);
        }
    }

    /// <summary>
    ///     Rule的规则，用于判断fact和数值之间的关系。
    ///     请特别注意：倘若criteria包含值，那么会忽略除了criteria和result之外的所有值。
    /// </summary>
    [Serializable]
    public struct RuleCriterion : IEquatable<RuleCriterion>
    {
        public bool isResultReversed;
        public bool isGlobal;
        public FactEntry fact;
        public CriteriaCompare compare;
        public int detection;

        public RuleLogicalOperation operation;

        public List<RuleCriterion> criteria;

        public bool GetResult()
        {
            return GetResult(isResultReversed, isGlobal, fact, compare, detection, criteria);
        }

        /// <summary>
        ///     封装，为Editor调用
        /// </summary>
        public static bool GetResult(bool isResultReversed,
            bool isGlobal,
            FactEntry fact,
            CriteriaCompare compare,
            int detection,
            List<RuleCriterion> criteria)
        {
            if (criteria.Count == 0)
            {
                if (!isGlobal)
                {
                    var isGetLocal = true;
                    if (!EventController.factTable)
                    {
                        isGetLocal = EventController.LoadTables(true);
                    }

                    if (!isGetLocal)
                    {
                        isGlobal = true;
                    }
                    else
                    {
                        var factTable = EventController.factTable.facts;
                        fact.value = GetFactsValue(fact, factTable);
                    }
                }

                if (isGlobal)
                {
                    var globalFactTable = EventController.globalFactTable.facts;
                    fact.value = GetFactsValue(fact, globalFactTable);
                }

                var result = compare switch
                {
                    CriteriaCompare.GreaterThan => fact.value > detection,
                    CriteriaCompare.GreaterThanOrEqual => fact.value >= detection,
                    CriteriaCompare.Equal => fact.value == detection,
                    CriteriaCompare.NotEqual => fact.value != detection,
                    CriteriaCompare.LessThanOrEqual => fact.value <= detection,
                    CriteriaCompare.LessThan => fact.value < detection,
                    _ => throw new ArgumentOutOfRangeException($"Unexpected compare value: {compare}")
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
                    case RuleLogicalOperation.None:
                    {
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException($"Unexpected operations value: {operations[i - 1]}");
                    }
                }

                if (operations[i - 1] == RuleLogicalOperation.None)
                {
                    InvalidOperationLogError(i, "None");
                }

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
                    case RuleLogicalOperation.None:
                    {
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException($"Unexpected operations value: {operations[^1]}");
                    }
                }
            }

            return isResultReversed ? !finalResult : finalResult;
        }

        private static int GetFactsValue(FactEntry fact, List<FactEntry> facts)
        {
            var value = 0;
            for (var i = 0; i < facts.Count; i++)
            {
                if (facts[i].name != fact.name)
                {
                    continue;
                }

                value = facts[i].value;
                break;
            }

            return value;
        }

        private static void InvalidOperationLogError(int i, string name)
        {
            Other.Debug.LogWarning(
                $"Invalid operation '{name}' at index {i - 1}. This will cause calculation anomalies.");
        }

        public bool Equals(RuleCriterion other)
        {
            return isResultReversed == other.isResultReversed && isGlobal == other.isGlobal &&
                   fact.Equals(other.fact) && compare == other.compare && detection == other.detection &&
                   operation == other.operation && Equals(criteria, other.criteria);
        }

        public override bool Equals(object obj)
        {
            return obj is RuleCriterion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(isResultReversed, isGlobal, fact, (int)compare, detection, (int)operation,
                criteria);
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
}