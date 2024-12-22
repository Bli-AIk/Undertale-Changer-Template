using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UCT.EventSystem
{
    /// <summary>
    /// 事件系统的Rult条目，被其他Event调用，同时可以触发其他Event与执行具体方法。
    /// </summary>
    [Serializable]
    public struct RuleEntry
    {
        public string name;
        public EventEntry triggeredBy;
        public EventEntry triggers;
        public RulePriority rulePriority;
        public Action Execute;

        public List<RuleCriterion> criteria;
        public List<Func<FactEntry, int>> Modifications; //TODO: 修改Fact
       
        //TODO: 检测与或非和括号
        public bool GetCriteria(RuleCriterion ruleCriterion)
        {
            var detection = ruleCriterion.detection;
            var compare = ruleCriterion.compare;
            var value = ruleCriterion.fact.value;
            return compare switch
            {
                CriteriaCompare.GreaterThan => value > detection,
                CriteriaCompare.GreaterThanOrEqual => value >= detection,
                CriteriaCompare.Equal => value == detection,
                CriteriaCompare.LessThanOrEqual => value <= detection,
                CriteriaCompare.LessThan => value < detection,
                _ => throw new ArgumentOutOfRangeException(nameof(compare), compare, null)
            };
        }

        public void ChangeFact(FactEntry fact, int newValue)
        {
            fact.value = newValue;
        }
        
        public void AddFact(FactEntry fact, int valueToAdd)
        {
            fact.value += valueToAdd;
        }

        public void SubtractFact(FactEntry fact, int valueToSubtract)
        {
            fact.value -= valueToSubtract;
        }

        public void MultiplyFact(FactEntry fact, int valueToMultiply)
        {
            fact.value *= valueToMultiply;
        }

        public void DivideFact(FactEntry fact, int valueToDivide)
        {
            if (valueToDivide != 0)
            {
                fact.value /= valueToDivide;
            }
            else
            {
                throw new DivideByZeroException("Fact cannot divide by zero.");
            }
        }

    }

    public enum RulePriority
    {
        Low,
        Medium,
        High
    }

    public enum CriteriaCompare
    {
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        LessThanOrEqual,
        LessThan
    }
}